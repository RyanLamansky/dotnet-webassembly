namespace WebAssembly.Runtime;

/// <summary>
/// Describes an exception where an out-of-range memory access was attempted.
/// </summary>
/// <param name="offset">The memory location that was attempted to be accessed.</param>
/// <param name="length">The amount of memory to be accessed.</param>
public class MemoryAccessOutOfRangeException(uint offset, uint length)
    : RuntimeException($"Attempted to access {length} bytes of memory starting at offset {offset}, which would have exceeded the allocated memory.")
{

    /// <summary>
    /// The memory location that was attempted to be accessed.
    /// </summary>
    public uint Offset { get; } = offset;

    /// <summary>
    /// The amount of memory to be accessed.
    /// </summary>
    public uint Length { get; } = length;
}
