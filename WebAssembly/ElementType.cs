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
}
