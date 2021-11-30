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
    }

    /// <summary>
    /// Initial length (in units of table elements or 65,536-byte pages).
    /// </summary>
    public uint Minimum { get; set; }

    /// <summary>
    /// Maximum length (in units of table elements or 65,536-byte pages).
    /// </summary>
    public uint? Maximum { get; set; }

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
        this.Minimum = minimum;
        this.Maximum = maximum;
    }

    /// <summary>
    /// Creates a new <see cref="ResizableLimits"/> from a binary data stream.
    /// </summary>
    /// <param name="reader">The source of data.</param>
    /// <exception cref="ArgumentNullException"><paramref name="reader"/> cannot be null.</exception>
    internal ResizableLimits(Reader reader)
    {
        if (reader == null)
            throw new ArgumentNullException(nameof(reader));

        var setFlags = (Flags)reader.ReadVarUInt32();
        this.Minimum = reader.ReadVarUInt32();
        if ((setFlags & Flags.Maximum) != 0)
            this.Maximum = reader.ReadVarUInt32();
    }

    /// <summary>
    /// Expresses the value of this instance as a string.
    /// </summary>
    /// <returns>A string representation of this instance.</returns>
    public override string ToString() => $"Minimum: {Minimum}, Maximum: {Maximum}";

    internal void WriteTo(Writer writer)
    {
        var flags = Flags.None
            | (this.Maximum.HasValue ? Flags.Maximum : 0)
            ;
        writer.WriteVar((uint)flags);
        writer.WriteVar(this.Minimum);
        if ((flags & Flags.Maximum) != 0)
            writer.WriteVar(this.Maximum.GetValueOrDefault());
    }
}
