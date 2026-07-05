using System;

namespace WebAssembly;

/// <summary>
/// A packed tuple that describes the limits of a <see cref="Table"/> or memory.
/// </summary>
public class ResizableLimits
{
    [Flags]
    internal enum Flags : uint
    {
        /// <summary>
        /// No flags are set.
        /// </summary>
        None = 0x0,
        /// <summary>
        /// Indicates whether the <see cref="ResizableLimits.Maximum"/> field is present.
        /// </summary>
        Maximum = 0x1,
        /// <summary>
        /// Indicates that the table or memory uses a 64-bit (i64) address/index type (WebAssembly 3.0
        /// memory64 / table64). When clear, addressing is 32-bit (i32).
        /// </summary>
        Is64Bit = 0x4,
    }

    /// <summary>
    /// Initial length (in units of table elements or 65,536-byte pages). This is the 32-bit view; for a 64-bit
    /// (memory64 / table64) limit whose value can exceed <see cref="uint.MaxValue"/>, use <see cref="Minimum64"/>.
    /// </summary>
    /// <exception cref="OverflowException">The value exceeds <see cref="uint.MaxValue"/>; read <see cref="Minimum64"/> instead.</exception>
    public uint Minimum
    {
        get => checked((uint)this.Minimum64);
        set => this.Minimum64 = value;
    }

    /// <summary>
    /// Maximum length (in units of table elements or 65,536-byte pages). This is the 32-bit view; for a 64-bit
    /// (memory64 / table64) limit whose value can exceed <see cref="uint.MaxValue"/>, use <see cref="Maximum64"/>.
    /// </summary>
    /// <exception cref="OverflowException">The value exceeds <see cref="uint.MaxValue"/>; read <see cref="Maximum64"/> instead.</exception>
    public uint? Maximum
    {
        get => this.Maximum64 is ulong m ? checked((uint)m) : null;
        set => this.Maximum64 = value;
    }

    /// <summary>
    /// Initial length at full 64-bit width, for memory64 / table64 limits. For a 32-bit limit this holds the
    /// same value as <see cref="Minimum"/>.
    /// </summary>
    public ulong Minimum64 { get; set; }

    /// <summary>
    /// Maximum length at full 64-bit width, for memory64 / table64 limits. For a 32-bit limit this holds the
    /// same value as <see cref="Maximum"/>.
    /// </summary>
    public ulong? Maximum64 { get; set; }

    // The runtime's tables and memories are uint-bounded, so compilation clamps (never truncates) an oversized
    // 64-bit declared bound: a minimum above uint.MaxValue can only ever fail to be satisfied, and clamping
    // preserves that, while truncation could yield a small value that wrongly succeeds.
    internal uint ClampedMinimum => (uint)Math.Min(this.Minimum64, uint.MaxValue);
    internal uint? ClampedMaximum => this.Maximum64 is ulong m ? (uint)Math.Min(m, uint.MaxValue) : null;

    /// <summary>
    /// When true, the table or memory uses a 64-bit (i64) address/index type (WebAssembly 3.0 memory64 / table64);
    /// when false, addressing is 32-bit (i32). This selects the operand type of every access, size, and growth
    /// operation targeting the associated table or memory.
    /// </summary>
    public bool Is64Bit { get; set; }

    /// <summary>
    /// Creates a new <see cref="ResizableLimits"/> instance.
    /// </summary>
    public ResizableLimits()
    {
    }

    /// <summary>
    /// Creates a new <see cref="ResizableLimits"/> instance with the provided <see cref="Minimum"/> and <see cref="Maximum"/> values.
    /// </summary>
    /// <param name="minimum">Initial length (in units of table elements or 65,536-byte pages).</param>
    /// <param name="maximum">Maximum length (in units of table elements or 65,536-byte pages).</param>
    public ResizableLimits(uint minimum, uint? maximum = null)
    {
        this.Minimum64 = minimum;
        this.Maximum64 = maximum;
    }

    /// <summary>
    /// Creates a new <see cref="ResizableLimits"/> instance with the provided full-width <see cref="Minimum64"/>
    /// and <see cref="Maximum64"/> values and address type.
    /// </summary>
    /// <param name="minimum">Initial length (in units of table elements or 65,536-byte pages).</param>
    /// <param name="maximum">Maximum length (in units of table elements or 65,536-byte pages).</param>
    /// <param name="is64Bit">When true, the table or memory uses a 64-bit (i64) address/index type.</param>
    public ResizableLimits(ulong minimum, ulong? maximum, bool is64Bit)
    {
        this.Minimum64 = minimum;
        this.Maximum64 = maximum;
        this.Is64Bit = is64Bit;
    }

    /// <summary>
    /// Creates a new <see cref="ResizableLimits"/> from a binary data stream.
    /// </summary>
    /// <param name="reader">The source of data.</param>
    /// <exception cref="ArgumentNullException"><paramref name="reader"/> cannot be null.</exception>
    internal ResizableLimits(Reader reader)
    {
        var preOffset = reader.Offset;
        var setFlags = (Flags)reader.ReadVarUInt32();
        this.Is64Bit = (setFlags & Flags.Is64Bit) != 0;
        // Read at full 64-bit width regardless of the address type: a 32-bit limit's LEB128 encoding is a prefix
        // of the 64-bit one, so this reads i32 values unchanged while also accepting the memory64 / table64 range.
        this.Minimum64 = reader.ReadVarUInt64();
        if ((setFlags & Flags.Maximum) != 0)
        {
            this.Maximum64 = reader.ReadVarUInt64();
            if (this.Maximum64 < this.Minimum64)
                throw new ModuleLoadException($"Size minimum of {this.Minimum64} must not be greater than maximum of {this.Maximum64}.", preOffset);
        }
    }

    /// <summary>
    /// Expresses the value of this instance as a string.
    /// </summary>
    /// <returns>A string representation of this instance.</returns>
    public override string ToString() => $"Minimum: {Minimum64}, Maximum: {Maximum64}{(Is64Bit ? ", i64" : "")}";

    internal void WriteTo(Writer writer)
    {
        var flags = Flags.None
            | (this.Maximum64.HasValue ? Flags.Maximum : 0)
            | (this.Is64Bit ? Flags.Is64Bit : 0)
            ;
        writer.WriteVar((uint)flags);
        // A value that fits in 32 bits encodes identically whether written as a u32 or u64 LEB128, so a 32-bit
        // limit round-trips byte-for-byte through the ulong path.
        writer.WriteVar(this.Minimum64);
        if ((flags & Flags.Maximum) != 0)
            writer.WriteVar(this.Maximum64.GetValueOrDefault());
    }
}
