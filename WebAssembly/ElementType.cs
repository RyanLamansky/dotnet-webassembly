namespace WebAssembly;

/// <summary>
/// The types of elements in a table.
/// </summary>
public enum ElementType : sbyte
{
    /// <summary>
    /// A function with any signature.
    /// </summary>
    FunctionReference = -0x10,
}
