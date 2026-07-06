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
    /// is the module's type section, consulted only for structural comparison of concrete-index references, and
    /// <paramref name="canonical"/> is its iso-recursive canonicalization (WebAssembly 3.0), used to decide
    /// concrete-index equality and declared subtyping. When <paramref name="canonical"/> is null the pre-3.0
    /// structural rule applies.
    /// </summary>
    public static bool IsSubtype(ValType sub, ValType super, Signature[]? types, CanonicalTypeMap? canonical = null)
    {
        if (sub.IsReference != super.IsReference)
            return false;

        if (!sub.IsReference)
            return sub.Numeric == super.Numeric;

        // A nullable reference is a subtype only of a nullable reference; a non-null one fits either.
        if (sub.IsNullable && !super.IsNullable)
            return false;

        return HeapSubtype(sub, super, types, canonical);
    }

    private static bool HeapSubtype(ValType sub, ValType super, Signature[]? types, CanonicalTypeMap? canonical)
    {
        if (sub.IsIndexed && super.IsIndexed)
        {
            // Iso-recursive identity plus declared single inheritance (canonicalization respects rec-group
            // boundaries and member position); fall back to raw structural equality only pre-canonicalization.
            return canonical is { } map
                ? map.IsDeclaredSubtype(sub.TypeIndex, super.TypeIndex)
                : StructurallyEqual(sub.TypeIndex, super.TypeIndex, types, null);
        }

        // A concrete type is a subtype of the abstract top(s) of its own hierarchy.
        if (sub.IsIndexed)
            return ConcreteSubtypeOfAbstract(ConcreteKind(sub.TypeIndex, types, canonical), super.HeapType);

        // The bottom of a hierarchy is a subtype of every concrete type in that hierarchy.
        if (super.IsIndexed)
        {
            return ConcreteKind(super.TypeIndex, types, canonical) switch
            {
                FunctionType.Struct or FunctionType.Array => sub.HeapType == WebAssemblyHeapType.None,
                _ => sub.HeapType == WebAssemblyHeapType.NoFunc,
            };
        }

        return AbstractHeapSubtype(sub.HeapType, super.HeapType);
    }

    // The composite kind of a concrete type index. Without a canonical map (pre-3.0) every concrete type is a
    // function; with one, struct/array indices report their real kind so the abstract lattice routes correctly.
    private static FunctionType ConcreteKind(uint index, Signature[]? types, CanonicalTypeMap? canonical) =>
        canonical is { } map && index < (uint)map.Kind.Length ? map.Kind[index] : FunctionType.Function;

    private static bool ConcreteSubtypeOfAbstract(FunctionType kind, WebAssemblyHeapType super) => kind switch
    {
        FunctionType.Struct => super is WebAssemblyHeapType.Struct or WebAssemblyHeapType.Eq or WebAssemblyHeapType.Any,
        FunctionType.Array => super is WebAssemblyHeapType.Array or WebAssemblyHeapType.Eq or WebAssemblyHeapType.Any,
        _ => super == WebAssemblyHeapType.Func,
    };

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
    /// (single-type rec-group) types terminate.
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
