using System.Collections.Generic;

namespace WebAssembly.Runtime.Compilation;

/// <summary>
/// The WebAssembly 3.0 subtyping lattice for <see cref="ValType"/>. For numeric/vector types subtyping reduces to
/// equality, so the validator's behavior on pre-3.0 modules is unchanged; references gain the func/extern/exn/any
/// heap-type hierarchies plus a structural rule over concrete function-type indices.
/// </summary>
internal static class TypeAlgebra
{
    /// <summary>
    /// Determines whether <paramref name="sub"/> is a subtype of <paramref name="super"/>. <paramref name="types"/>
    /// is the module's type section, consulted only for structural comparison of concrete-index references.
    /// </summary>
    public static bool IsSubtype(ValType sub, ValType super, Signature[]? types)
    {
        if (sub.IsReference != super.IsReference)
            return false;

        if (!sub.IsReference)
            return sub.Numeric == super.Numeric;

        // A nullable reference is a subtype only of a nullable reference; a non-null one fits either.
        if (sub.IsNullable && !super.IsNullable)
            return false;

        return HeapSubtype(sub, super, types);
    }

    private static bool HeapSubtype(ValType sub, ValType super, Signature[]? types)
    {
        if (sub.IsIndexed && super.IsIndexed)
            return StructurallyEqual(sub.TypeIndex, super.TypeIndex, types, null);

        // A concrete function type is a subtype of func (and, reflexively, of itself, handled above).
        if (sub.IsIndexed)
            return super.HeapType == WebAssemblyHeapType.Func;

        // nofunc is the bottom of the func hierarchy, so it is a subtype of every concrete function type.
        if (super.IsIndexed)
            return sub.HeapType == WebAssemblyHeapType.NoFunc;

        return AbstractHeapSubtype(sub.HeapType, super.HeapType);
    }

    private static bool AbstractHeapSubtype(WebAssemblyHeapType a, WebAssemblyHeapType b)
    {
        if (a == b)
            return true;

        return b switch
        {
            WebAssemblyHeapType.Func => a == WebAssemblyHeapType.NoFunc,
            WebAssemblyHeapType.Extern => a == WebAssemblyHeapType.NoExtern,
            WebAssemblyHeapType.Exn => a == WebAssemblyHeapType.NoExn,
            WebAssemblyHeapType.Any => a is WebAssemblyHeapType.None
                or WebAssemblyHeapType.I31 or WebAssemblyHeapType.Struct or WebAssemblyHeapType.Array
                or WebAssemblyHeapType.Eq,
            WebAssemblyHeapType.Eq => a is WebAssemblyHeapType.None
                or WebAssemblyHeapType.I31 or WebAssemblyHeapType.Struct or WebAssemblyHeapType.Array,
            WebAssemblyHeapType.I31 or WebAssemblyHeapType.Struct or WebAssemblyHeapType.Array
                => a == WebAssemblyHeapType.None,
            _ => false,
        };
    }

    /// <summary>
    /// Structural equality of two concrete function types, with a bisimulation assumption set so self-referential
    /// (single-type rec-group) types terminate. Phase 5 fixtures do not exercise recursion, but the machinery is
    /// cheap and Phase 8 requires it.
    /// </summary>
    private static bool StructurallyEqual(uint i, uint j, Signature[]? types, HashSet<(uint, uint)>? assume)
    {
        if (i == j)
            return true;
        if (types is null || i >= (uint)types.Length || j >= (uint)types.Length)
            return false;

        assume ??= [];
        if (!assume.Add((i, j)))
            return true; // Already assumed equal along this path — bisimulation closes the cycle.

        var a = types[i];
        var b = types[j];
        if (a.RawParameterTypes.Length != b.RawParameterTypes.Length)
            return false;
        if (a.RawReturnTypes.Length != b.RawReturnTypes.Length)
            return false;

        for (var k = 0; k < a.RawParameterTypes.Length; k++)
        {
            if (!ElementEqual(a.RawParameterTypes[k], b.RawParameterTypes[k], types, assume))
                return false;
        }

        for (var k = 0; k < a.RawReturnTypes.Length; k++)
        {
            if (!ElementEqual(a.RawReturnTypes[k], b.RawReturnTypes[k], types, assume))
                return false;
        }

        return true;
    }

    private static bool ElementEqual(ValType a, ValType b, Signature[] types, HashSet<(uint, uint)> assume)
    {
        if (a.IsReference != b.IsReference)
            return false;
        if (!a.IsReference)
            return a.Numeric == b.Numeric;
        if (a.IsNullable != b.IsNullable || a.IsIndexed != b.IsIndexed)
            return false;
        return a.IsIndexed
            ? StructurallyEqual(a.TypeIndex, b.TypeIndex, types, assume)
            : a.HeapType == b.HeapType;
    }
}
