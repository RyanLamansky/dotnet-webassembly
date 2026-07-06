using System;
using System.Collections.Generic;
using System.Text;

namespace WebAssembly.Runtime.Compilation;

/// <summary>
/// A closed, canonical string rendering of a reference type's WebAssembly structure, used to link globals and
/// tables across compiled-module boundaries. The CLR representation is too coarse for this: every func-hierarchy
/// reference is a <see cref="Delegate"/> and every other reference is <see cref="object"/>, so structurally-distinct
/// reference types (for example <c>(ref $f)</c> versus <c>funcref</c>) are indistinguishable at run time.
/// </summary>
/// <remarks>
/// A descriptor is <em>closed</em>: every concrete type index is expanded recursively into its structural function
/// signature (with a back-reference cycle guard), so two independently-compiled modules that agree on a type
/// structurally produce byte-identical descriptor strings, and no cross-module type index ever escapes. Only
/// reference types get a descriptor; numeric/vector types render as <c>null</c>, which keeps the existing coarse
/// CLR-type import check in force for them (and for every host-supplied import, which carries no descriptor).
/// <para>
/// Grammar: <c>ref [null ]heap</c>, where <c>heap</c> is either an abstract name (<c>func</c>, <c>extern</c>,
/// <c>any</c>, <c>eq</c>, <c>i31</c>, <c>struct</c>, <c>array</c>, <c>none</c>, <c>nofunc</c>, <c>noextern</c>,
/// <c>exn</c>, <c>noexn</c>) or a concrete function type <c>func(v,…)->(v,…)</c> whose element <c>v</c> is itself a
/// value descriptor (<c>i32</c>/<c>i64</c>/<c>f32</c>/<c>f64</c>/<c>v128</c> or a nested <c>ref …</c>). Because the
/// rendering is canonical, invariant (exact) matching reduces to string equality; subtyping is
/// <see cref="IsSubtype"/>.
/// </para>
/// </remarks>
internal static class WasmTypeDescriptor
{
    /// <summary>
    /// Renders <paramref name="type"/> to its canonical descriptor string, or returns <c>null</c> when it is a
    /// numeric/vector type (which needs no descriptor). <paramref name="types"/> is the module's type section,
    /// consulted to expand concrete-index references to their structural form.
    /// </summary>
    public static string? Render(ValType type, Signature[]? types)
    {
        if (!type.IsReference)
            return null;

        var builder = new StringBuilder();
        AppendValue(builder, type, types, null);
        return builder.ToString();
    }

    /// <summary>
    /// Determines whether the type described by <paramref name="sub"/> is a subtype of the one described by
    /// <paramref name="super"/> (used for immutable-global linking, where the supply need only be a subtype).
    /// </summary>
    public static bool IsSubtype(string sub, string super)
    {
        if (sub == super)
            return true;

        if (!TryParseRef(sub, out var subNullable, out var subHeap) ||
            !TryParseRef(super, out var superNullable, out var superHeap))
            return false; // Non-reference descriptors (unexpected here) compare only by exact string.

        // A nullable reference is a subtype only of a nullable reference; a non-null one fits either.
        if (subNullable && !superNullable)
            return false;

        return HeapSubtype(subHeap, superHeap);
    }

    private static void AppendValue(StringBuilder builder, ValType type, Signature[]? types, List<uint>? path)
    {
        if (!type.IsReference)
        {
            builder.Append(NumericName(type.Numeric));
            return;
        }

        builder.Append("ref ");
        if (type.IsNullable)
            builder.Append("null ");

        if (type.IsIndexed)
            AppendConcrete(builder, type.TypeIndex, types, path);
        else
            builder.Append(AbstractName(type.HeapType));
    }

    private static void AppendConcrete(StringBuilder builder, uint index, Signature[]? types, List<uint>? path)
    {
        // Cycle guard: a self-referential type (a single-type rec group is legal 3.0) renders as a back-reference
        // to its position on the expansion path, so it terminates and two identical cycles render identically.
        path ??= [];
        var existing = path.IndexOf(index);
        if (existing >= 0)
        {
            builder.Append("rec").Append(path.Count - existing);
            return;
        }

        if (types is null || index >= (uint)types.Length)
        {
            // No structural detail available (should not occur for a well-formed module).
            builder.Append("func#").Append(index);
            return;
        }

        path.Add(index);
        var signature = types[(int)index];

        builder.Append("func(");
        for (var i = 0; i < signature.RawParameterTypes.Length; i++)
        {
            if (i != 0)
                builder.Append(',');
            AppendValue(builder, signature.RawParameterTypes[i], types, path);
        }

        builder.Append(")->(");
        for (var i = 0; i < signature.RawReturnTypes.Length; i++)
        {
            if (i != 0)
                builder.Append(',');
            AppendValue(builder, signature.RawReturnTypes[i], types, path);
        }

        builder.Append(')');
        path.RemoveAt(path.Count - 1);
    }

    private static bool TryParseRef(string descriptor, out bool nullable, out string heap)
    {
        if (!descriptor.StartsWith("ref ", StringComparison.Ordinal))
        {
            nullable = false;
            heap = descriptor;
            return false;
        }

        var rest = descriptor[4..];
        if (rest.StartsWith("null ", StringComparison.Ordinal))
        {
            nullable = true;
            heap = rest[5..];
        }
        else
        {
            nullable = false;
            heap = rest;
        }

        return true;
    }

    private static bool HeapSubtype(string sub, string super)
    {
        if (sub == super)
            return true;

        return super switch
        {
            // A concrete function type or nofunc is a subtype of abstract func.
            "func" => sub == "nofunc" || IsConcreteFunc(sub),
            "extern" => sub == "noextern",
            "exn" => sub == "noexn",
            "any" => sub is "none" or "i31" or "struct" or "array" or "eq",
            "eq" => sub is "none" or "i31" or "struct" or "array",
            "i31" or "struct" or "array" => sub == "none",
            // A concrete function type: distinct concrete functions are subtypes only when structurally equal
            // (identical strings, handled above); otherwise only nofunc, the func-hierarchy bottom, qualifies.
            _ => IsConcreteFunc(super) && sub == "nofunc",
        };
    }

    private static bool IsConcreteFunc(string heap) => heap.StartsWith("func(", StringComparison.Ordinal);

    private static string NumericName(WebAssemblyValueType numeric) => numeric switch
    {
        WebAssemblyValueType.Int32 => "i32",
        WebAssemblyValueType.Int64 => "i64",
        WebAssemblyValueType.Float32 => "f32",
        WebAssemblyValueType.Float64 => "f64",
        WebAssemblyValueType.V128 => "v128",
        _ => numeric.ToString(),
    };

    private static string AbstractName(WebAssemblyHeapType heapType) => heapType.ToString().ToLowerInvariant();
}
