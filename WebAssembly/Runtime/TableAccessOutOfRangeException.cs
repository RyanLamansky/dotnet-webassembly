namespace WebAssembly.Runtime;

/// <summary>
/// Describes an exception where an out-of-range table access was attempted.
/// </summary>
/// <param name="index">The table index that was attempted to be accessed.</param>
/// <param name="length">The number of elements to be accessed.</param>
public class TableAccessOutOfRangeException(uint index, uint length)
    : RuntimeException($"Attempted to access {length} table elements starting at index {index}, which would have exceeded the table bounds.")
{
    /// <summary>
    /// The table index that was attempted to be accessed.
    /// </summary>
    public uint Index { get; } = index;

    /// <summary>
    /// The number of elements to be accessed.
    /// </summary>
    public uint Length { get; } = length;
}
