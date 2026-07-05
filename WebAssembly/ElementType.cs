namespace WebAssembly;

/// <summary>
/// The types of elements in a table.
/// </summary>
public enum ElementType : sbyte
{
    /// <summary>
    /// A function with any signature (funcref).
    /// </summary>
    FunctionReference = -0x10,

    /// <summary>
    /// An external host reference (externref).
    /// </summary>
    ExternRef = -0x11,

    /// <summary>
    /// A non-null typed reference (WASM 3.0), encoded as 0x64 followed by an s33 heap type.
    /// The heap type detail is carried in a companion <see cref="ReferenceType"/>.
    /// </summary>
    Reference = -0x1C,

    /// <summary>
    /// A nullable typed reference (WASM 3.0), encoded as 0x63 followed by an s33 heap type.
    /// The heap type detail is carried in a companion <see cref="ReferenceType"/>.
    /// </summary>
    NullableReference = -0x1D,
}
