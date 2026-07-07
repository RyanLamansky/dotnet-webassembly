using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using WebAssembly.Instructions;
using ILOpCode = System.Reflection.Emit.OpCode;

namespace WebAssembly.Runtime;

/// <summary>
/// Guards the WebAssembly per-call hot paths against silent heap allocations. Compiled modules invoke a fixed
/// set of runtime helpers once per executed instruction — the <c>public static Execute</c> methods of every
/// <see cref="SimdInstruction"/>, plus a handful of <see cref="WebAssemblyGC"/> reference-type helpers. A stray
/// <c>new T[]</c> (rather than a <c>stackalloc</c> span), a <c>box</c>, or a <c>new</c> of a reference type in any
/// of these turns a zero-allocation instruction into a per-call garbage generator. This test walks each method's
/// IL and fails if it finds a heap-allocating opcode, so such a regression can't slip in unnoticed.
/// </summary>
[TestClass]
public class HotPathAllocationGuardTests
{
    // WebAssemblyGC helpers reachable from per-call compiled code that must stay allocation-free. The array
    // construction helpers (NewArray, NewArrayFromData, NewArrayFromElement, FillArray, ...) are deliberately
    // excluded: array.new/array.fill/etc. allocate by design.
    private static readonly string[] hotGCHelpers =
    [
        nameof(WebAssemblyGC.RefEqual),
        nameof(WebAssemblyGC.CheckIndirectCallType),
        nameof(WebAssemblyGC.CheckIndirectCallTypeCached),
        nameof(WebAssemblyGC.IsFunctionReferenceOfType),
        nameof(WebAssemblyGC.CastFunctionReference),
    ];

    /// <summary>
    /// Methods with a justified, reviewed heap allocation are listed here (by "DeclaringType.MethodName") and
    /// skipped. Empty today: every guarded hot-path method is allocation-free. Adding an entry requires a comment
    /// explaining why the allocation is unavoidable and acceptable on that path.
    /// </summary>
    private static readonly HashSet<string> exemptions = new(StringComparer.Ordinal)
    {
        // (none)
    };

    private static IEnumerable<MethodInfo> HotPathMethods()
    {
        var assembly = typeof(SimdInstruction).Assembly;

        // Every public static Execute on a SimdInstruction (base or leaf); each is a per-call helper.
        foreach (var type in assembly.GetTypes())
        {
            if (!typeof(SimdInstruction).IsAssignableFrom(type))
                continue;

            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly))
                if (method.Name == "Execute")
                    yield return method;
        }

        foreach (var name in hotGCHelpers)
            yield return typeof(WebAssemblyGC).GetMethod(name, BindingFlags.Public | BindingFlags.Static)
                ?? throw new InvalidOperationException($"WebAssemblyGC.{name} was not found; update the hot-path guard.");
    }

    /// <summary>The single-byte and two-byte (0xFE-prefixed) IL opcodes, keyed by their <see cref="ILOpCode.Value"/>.</summary>
    private static readonly Dictionary<short, ILOpCode> opcodesByValue =
        typeof(OpCodes)
        .GetFields(BindingFlags.Public | BindingFlags.Static)
        .Where(f => f.FieldType == typeof(ILOpCode))
        .Select(f => (ILOpCode)f.GetValue(null)!)
        .ToDictionary(op => op.Value);

    /// <summary>Advances past an opcode's inline operand, returning the number of operand bytes consumed.</summary>
    private static int OperandSize(ILOpCode op, byte[] il, int operandStart) => op.OperandType switch
    {
        OperandType.InlineNone => 0,
        OperandType.ShortInlineBrTarget or OperandType.ShortInlineI or OperandType.ShortInlineVar => 1,
        OperandType.InlineVar => 2,
        OperandType.InlineBrTarget or OperandType.InlineField or OperandType.InlineI or OperandType.InlineMethod
            or OperandType.InlineSig or OperandType.InlineString or OperandType.InlineTok or OperandType.InlineType
            or OperandType.ShortInlineR => 4,
        OperandType.InlineI8 or OperandType.InlineR => 8,
        // switch: a 4-byte case count followed by that many 4-byte relative targets.
        OperandType.InlineSwitch => 4 + 4 * BitConverter.ToInt32(il, operandStart),
        _ => throw new InvalidOperationException($"Unhandled operand type {op.OperandType} for {op.Name}."),
    };

    private static IEnumerable<string> FindHeapAllocatingOpcodes(MethodInfo method)
    {
        var body = method.GetMethodBody();
        if (body is null)
            yield break;

        var il = body.GetILAsByteArray();
        if (il is null)
            yield break;

        var typeArgs = method.DeclaringType is { IsGenericType: true } dt ? dt.GetGenericArguments() : null;
        var methodArgs = method.IsGenericMethod ? method.GetGenericArguments() : null;

        var i = 0;
        while (i < il.Length)
        {
            byte first = il[i++];
            var value = first == 0xFE ? (short)(0xFE00 | il[i++]) : (short)first;
            if (!opcodesByValue.TryGetValue(value, out var op))
                throw new InvalidOperationException($"Unrecognized IL opcode 0x{value:X} in {method.DeclaringType!.Name}.{method.Name}.");

            var operandStart = i;

            if (op == OpCodes.Newarr)
                yield return $"newarr at IL_{operandStart - 1:X4}";
            else if (op == OpCodes.Box)
                yield return $"box at IL_{operandStart - 1:X4}";
            else if (op == OpCodes.Newobj)
            {
                var token = BitConverter.ToInt32(il, operandStart);
                var declaring = method.Module.ResolveMethod(token, typeArgs, methodArgs)?.DeclaringType;

                // newobj only allocates on the GC heap for reference types. Value types (including ref structs such
                // as Span<T> produced by stackalloc) construct in place. Exception construction is a trap/cold path.
                if (declaring is not null
                    && !declaring.IsValueType
                    && !typeof(Exception).IsAssignableFrom(declaring))
                    yield return $"newobj {declaring.Name} at IL_{operandStart - 1:X4}";
            }

            i += OperandSize(op, il, operandStart);
        }
    }

    /// <summary>Every guarded per-call hot-path method is free of heap-allocating IL (newarr / box / reference-type newobj).</summary>
    [TestMethod]
    public void HotPaths_ContainNoHeapAllocatingIL()
    {
        var methods = HotPathMethods().ToList();

        // Sanity: reflection must actually find the hot-path set, or the guard would silently pass.
        Assert.IsTrue(methods.Count > 100, $"Expected the SIMD Execute + GC helper set to be large; found only {methods.Count}.");

        var violations = new List<string>();
        foreach (var method in methods)
        {
            var key = $"{method.DeclaringType!.Name}.{method.Name}";
            if (exemptions.Contains(key))
                continue;

            foreach (var finding in FindHeapAllocatingOpcodes(method))
                violations.Add($"{key}: {finding}");
        }

        Assert.AreEqual(0, violations.Count,
            "Heap-allocating IL found on per-call hot paths (use stackalloc spans, avoid boxing/params arrays). " +
            "If an allocation is justified, add the method to the documented exemptions set.\n" +
            string.Join("\n", violations));
    }
}
