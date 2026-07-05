namespace WebAssembly.Runtime;

/// <summary>
/// Describes an exception where an out-of-range memory access was attempted.
/// </summary>
public class MemoryAccessOutOfRangeException
    : RuntimeException
{
    /// <summary>
    /// Creates a new <see cref="MemoryAccessOutOfRangeException"/> for a 32-bit access.
    /// </summary>
    /// <param name="offset">The memory location that was attempted to be accessed.</param>
    /// <param name="length">The amount of memory to be accessed.</param>
    public MemoryAccessOutOfRangeException(uint offset, uint length)
        : this((ulong)offset, length)
    {
    }

    /// <summary>
    /// Creates a new <see cref="MemoryAccessOutOfRangeException"/> for a 64-bit (memory64) access, where the
    /// offset or length may exceed <see cref="uint.MaxValue"/>.
    /// </summary>
    /// <param name="offset">The memory location that was attempted to be accessed.</param>
    /// <param name="length">The amount of memory to be accessed.</param>
    public MemoryAccessOutOfRangeException(ulong offset, ulong length)
        : base($"Attempted to access {length} bytes of memory starting at offset {offset}, which would have exceeded the allocated memory.")
    {
        this.Offset64 = offset;
        this.Length64 = length;
    }

    /// <summary>
    /// The memory location that was attempted to be accessed (32-bit view; see <see cref="Offset64"/> for the full width).
    /// </summary>
    /// <exception cref="System.OverflowException">The value exceeds <see cref="uint.MaxValue"/>; read <see cref="Offset64"/> instead.</exception>
    public uint Offset => checked((uint)this.Offset64);

    /// <summary>
    /// The amount of memory to be accessed (32-bit view; see <see cref="Length64"/> for the full width).
    /// </summary>
    /// <exception cref="System.OverflowException">The value exceeds <see cref="uint.MaxValue"/>; read <see cref="Length64"/> instead.</exception>
    public uint Length => checked((uint)this.Length64);

    /// <summary>
    /// The memory location that was attempted to be accessed, at full 64-bit width.
    /// </summary>
    public ulong Offset64 { get; }

    /// <summary>
    /// The amount of memory to be accessed, at full 64-bit width.
    /// </summary>
    public ulong Length64 { get; }
}
