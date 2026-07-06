namespace WebAssembly;

/// <summary>
/// The abstract heap types that a WebAssembly 3.0 reference type may target (in addition to a concrete type index).
/// Each member's value is the signed byte (s7) that encodes it in the binary format.
/// </summary>
/// <remarks>
/// All twelve are defined here as pure encoding facts: the binary encoding of the abstract heap types across the
/// function, external, garbage-collection (any/eq/i31/struct/array), and exception (exn) hierarchies, independent of
/// which reference-type semantics a given module exercises.
/// </remarks>
public enum WebAssemblyHeapType : sbyte
{
    /// <summary>The bottom of the exception hierarchy (noexn); encoded as 0x74.</summary>
    NoExn = -0x0C,
    /// <summary>The bottom of the function hierarchy (nofunc); encoded as 0x73.</summary>
    NoFunc = -0x0D,
    /// <summary>The bottom of the external hierarchy (noextern); encoded as 0x72.</summary>
    NoExtern = -0x0E,
    /// <summary>The bottom of the internal (any) hierarchy (none); encoded as 0x71.</summary>
    None = -0x0F,
    /// <summary>A function of any signature (func); encoded as 0x70.</summary>
    Func = -0x10,
    /// <summary>A host-provided external reference (extern); encoded as 0x6F.</summary>
    Extern = -0x11,
    /// <summary>The top of the internal hierarchy (any); encoded as 0x6E.</summary>
    Any = -0x12,
    /// <summary>A reference comparable with <c>ref.eq</c> (eq); encoded as 0x6D.</summary>
    Eq = -0x13,
    /// <summary>An unboxed 31-bit integer reference (i31); encoded as 0x6C.</summary>
    I31 = -0x14,
    /// <summary>A structure reference (struct); encoded as 0x6B.</summary>
    Struct = -0x15,
    /// <summary>An array reference (array); encoded as 0x6A.</summary>
    Array = -0x16,
    /// <summary>An exception reference (exn); encoded as 0x69.</summary>
    Exn = -0x17,
}

static class WebAssemblyHeapTypeExtensions
{
    public static bool IsDefined(this WebAssemblyHeapType heapType) => heapType switch
    {
        WebAssemblyHeapType.NoExn
        or WebAssemblyHeapType.NoFunc
        or WebAssemblyHeapType.NoExtern
        or WebAssemblyHeapType.None
        or WebAssemblyHeapType.Func
        or WebAssemblyHeapType.Extern
        or WebAssemblyHeapType.Any
        or WebAssemblyHeapType.Eq
        or WebAssemblyHeapType.I31
        or WebAssemblyHeapType.Struct
        or WebAssemblyHeapType.Array
        or WebAssemblyHeapType.Exn => true,
        _ => false,
    };
}
