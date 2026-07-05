using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Tail-call a function indirectly through a table (WASM 3.0): like <see cref="ReturnCallIndirect"/>'s direct
/// counterpart, the callee's results must exactly match the enclosing function's. Emitted as an IL
/// <c>tail.</c>-prefixed call to the resolved delegate followed by <c>ret</c>.
/// </summary>
public class ReturnCallIndirect : Instruction, IEquatable<ReturnCallIndirect>
{
    /// <summary>
    /// Always <see cref="OpCode.ReturnCallIndirect"/>.
    /// </summary>
    public sealed override OpCode OpCode => OpCode.ReturnCallIndirect;

    /// <summary>
    /// The index of the type representing the function signature.
    /// </summary>
    public uint Type { get; set; }

    /// <summary>
    /// The index of the table from which the indirect tail-call selects its target.
    /// </summary>
    public uint Table { get; set; }

    /// <summary>
    /// Creates a new <see cref="ReturnCallIndirect"/> instance.
    /// </summary>
    public ReturnCallIndirect()
    {
    }

    /// <summary>
    /// Creates a new <see cref="ReturnCallIndirect"/> instance.
    /// </summary>
    /// <param name="type">The index of the type representing the function signature.</param>
    public ReturnCallIndirect(uint type)
    {
        this.Type = type;
    }

    internal ReturnCallIndirect(Reader reader)
    {
        Type = reader.ReadVarUInt32();
        Table = reader.ReadVarUInt32();
    }

    internal sealed override void WriteTo(Writer writer)
    {
        writer.Write((byte)OpCode.ReturnCallIndirect);
        writer.WriteVar(this.Type);
        writer.WriteVar(this.Table);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => this.Equals(obj as ReturnCallIndirect);

    /// <summary>
    /// Determines whether this instruction is identical to another.
    /// </summary>
    /// <param name="other">The instruction to compare against.</param>
    /// <returns>True if they have the same type and value, otherwise false.</returns>
    public override bool Equals(Instruction? other) => this.Equals(other as ReturnCallIndirect);

    /// <summary>
    /// Determines whether this instruction is identical to another.
    /// </summary>
    /// <param name="other">The instruction to compare against.</param>
    /// <returns>True if they have the same type and value, otherwise false.</returns>
    public bool Equals(ReturnCallIndirect? other) =>
        other != null
        && other.Type == this.Type
        && other.Table == this.Table
        ;

    /// <summary>
    /// Returns a simple hash code based on the value of the instruction.
    /// </summary>
    /// <returns>The hash code.</returns>
    public override int GetHashCode() => HashCode.Combine((int)this.OpCode, (int)this.Type, (int)this.Table);

    internal sealed override void Compile(CompilationContext context)
    {
        if (this.Type >= context.CheckedTypes.Length)
            throw new ModuleLoadException($"unknown type {this.Type}: the module defines only {context.CheckedTypes.Length} type(s).", 0);

        // Resolves the table (throwing a ModuleLoadException for an out-of-range index) and rejects non-funcref tables.
        var (tableField, isFunc) = context.ResolveTable(this.Table);
        if (!isFunc)
            throw new ModuleLoadException($"return_call_indirect requires a funcref table; table {this.Table} holds external references.", 0);

        var signature = context.CheckedTypes[this.Type];
        var paramTypes = signature.RawParameterTypes;
        var returnTypes = signature.RawReturnTypes;

        // The callee's results must exactly match the enclosing function's results (the tail call returns to the
        // caller's caller). A mismatch is a "type mismatch" validation error.
        if (!returnTypes.SequenceEqual(context.CheckedSignature.RawReturnTypes))
            throw new ModuleLoadException(
                $"return_call_indirect target result types [{string.Join(", ", returnTypes)}] do not match the enclosing function's results [{string.Join(", ", context.CheckedSignature.RawReturnTypes)}].",
                0);

        var stackCountBefore = context.Stack.Count;

        // The table index (i32) sits on top of the arguments.
        context.PopStackNoReturn(this.OpCode, paramTypes.Cast<WebAssemblyValueType?>().Reverse().Prepend(WebAssemblyValueType.Int32), paramTypes.Length + 1);

        var parms = signature.ParameterTypes;
        var returns = signature.ReturnTypes;

        if (!context.DelegateInvokersByTypeIndex.TryGetValue(signature.TypeIndex, out var invoker))
        {
            // Two-or-more results map onto a delegate with a single (ValueTuple) return.
            var del = context.Configuration.GetDelegateForType(parms.Length, returns.Length > 1 ? 1 : returns.Length) ??
                throw new CompilerException($"Failed to get a delegate for type {signature}.");
            if (del.IsGenericType)
                del = del.MakeGenericType(MultiValueHelper.DelegateTypeArgs(parms, returns));
            context.DelegateInvokersByTypeIndex.Add(signature.TypeIndex, invoker = del.GetTypeInfo().GetDeclaredMethod(nameof(Action.Invoke))!);
        }

        // Spill the arguments and the table index; discard any operand-stack values beneath them (an IL tail-call
        // requires exactly the call arguments on the stack). Operand order is [args..., index], index on top.
        var operandTypes = new WebAssemblyValueType[paramTypes.Length + 1];
        Array.Copy(paramTypes, operandTypes, paramTypes.Length);
        operandTypes[paramTypes.Length] = WebAssemblyValueType.Int32;
        var locals = TailCallHelper.SpillOperands(context, operandTypes, stackCountBefore);
        var indexLocal = locals[paramTypes.Length];

        // Resolve the target delegate from the table: instance.table[index], cast to the concrete delegate type.
        context.EmitLoadThis();
        context.Emit(OpCodes.Ldfld, tableField);
        context.Emit(OpCodes.Ldloc, indexLocal);
        context.Emit(OpCodes.Call, FunctionTable.IndexGetter);
        context.Emit(OpCodes.Castclass, invoker.DeclaringType!);

        // Reload the arguments, then tail-call the delegate's Invoke directly so no intermediate frame survives.
        for (var i = 0; i < paramTypes.Length; i++)
            context.Emit(OpCodes.Ldloc, locals[i]);

        context.Emit(OpCodes.Tailcall);
        context.Emit(OpCodes.Call, invoker);
        context.Emit(OpCodes.Ret);

        // Everything after a tail call is unreachable, exactly like return.
        context.MarkUnreachable(functionWide: true);
    }
}
