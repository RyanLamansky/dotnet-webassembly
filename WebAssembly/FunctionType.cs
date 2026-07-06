namespace WebAssembly;

/// <summary>
/// Applied to <see cref="WebAssemblyType"/> to indicate the kind of composite type.
/// </summary>
public enum FunctionType : sbyte
{
    /// <summary>
    /// A function.  Encoded as <c>0x60</c>.
    /// </summary>
    Function = -0x20,
    /// <summary>
    /// A structure (WASM 3.0 garbage collection): an ordered collection of mutable-or-immutable fields.  Encoded as <c>0x5F</c>.
    /// </summary>
    Struct = -0x21,
    /// <summary>
    /// An array (WASM 3.0 garbage collection): a homogeneous sequence of a single element field type.  Encoded as <c>0x5E</c>.
    /// </summary>
    Array = -0x22,
}
