namespace WebAssembly.Runtime;

/// <summary>
/// Describes an exception where an out-of-range table access was attempted.
/// </summary>
public class TableAccessOutOfRangeException
    : RuntimeException
{
    /// <summary>
    /// Creates a new <see cref="TableAccessOutOfRangeException"/> for a 32-bit table.
    /// </summary>
    /// <param name="index">The table index that was attempted to be accessed.</param>
    /// <param name="length">The number of elements to be accessed.</param>
    public TableAccessOutOfRangeException(uint index, uint length)
        : this((ulong)index, length)
    {
    }

    /// <summary>
    /// Creates a new <see cref="TableAccessOutOfRangeException"/> for a 64-bit (table64) table, where the index
    /// or length may exceed <see cref="uint.MaxValue"/>.
    /// </summary>
    /// <param name="index">The table index that was attempted to be accessed.</param>
    /// <param name="length">The number of elements to be accessed.</param>
    public TableAccessOutOfRangeException(ulong index, ulong length)
        : base($"Attempted to access {length} table elements starting at index {index}, which would have exceeded the table bounds.")
    {
        this.Index64 = index;
        this.Length64 = length;
    }

    /// <summary>
    /// The table index that was attempted to be accessed (32-bit view; see <see cref="Index64"/> for the full width).
    /// </summary>
    /// <exception cref="System.OverflowException">The value exceeds <see cref="uint.MaxValue"/>; read <see cref="Index64"/> instead.</exception>
    public uint Index => checked((uint)this.Index64);

    /// <summary>
    /// The number of elements to be accessed (32-bit view; see <see cref="Length64"/> for the full width).
    /// </summary>
    /// <exception cref="System.OverflowException">The value exceeds <see cref="uint.MaxValue"/>; read <see cref="Length64"/> instead.</exception>
    public uint Length => checked((uint)this.Length64);

    /// <summary>
    /// The table index that was attempted to be accessed, at full 64-bit width.
    /// </summary>
    public ulong Index64 { get; }

    /// <summary>
    /// The number of elements to be accessed, at full 64-bit width.
    /// </summary>
    public ulong Length64 { get; }
}
