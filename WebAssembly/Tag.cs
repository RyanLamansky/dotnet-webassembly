using System;

namespace WebAssembly;

/// <summary>
/// Describes an exception tag (WASM 3.0 exception handling): a reference to a function-type signature (with empty
/// results) that describes the operands carried by exceptions thrown with this tag.
/// </summary>
public class Tag
{
    /// <summary>
    /// The index to the <see cref="Module.Types"/> entry that describes the exception signature. The referenced
    /// function type must have empty results.
    /// </summary>
    public uint TypeIndex { get; set; }

    /// <summary>
    /// Creates a new <see cref="Tag"/> instance.
    /// </summary>
    public Tag()
    {
    }

    /// <summary>
    /// Creates a new <see cref="Tag"/> instance with the provided type index.
    /// </summary>
    /// <param name="typeIndex">The index to the <see cref="Module.Types"/> entry that describes the exception signature.</param>
    public Tag(uint typeIndex)
    {
        this.TypeIndex = typeIndex;
    }

    /// <summary>
    /// Creates a new <see cref="Tag"/> from a binary data stream.
    /// </summary>
    /// <param name="reader">The source of data.</param>
    /// <exception cref="ArgumentNullException"><paramref name="reader"/> cannot be null.</exception>
    /// <exception cref="ModuleLoadException">The tag's attribute byte is not the (only defined) exception kind.</exception>
    internal Tag(Reader reader)
    {
        ArgumentNullException.ThrowIfNull(reader, nameof(reader));

        var attribute = reader.ReadByte();
        if (attribute != 0x00)
            throw new ModuleLoadException($"Unsupported tag attribute 0x{attribute:X2}; only 0x00 (exception) is defined.", reader.Offset);

        this.TypeIndex = reader.ReadVarUInt32();
    }

    /// <summary>
    /// Expresses the value of this instance as a string.
    /// </summary>
    /// <returns>A string representation of this instance.</returns>
    public override string ToString() => $"Tag {TypeIndex}";

    /// <summary>
    /// Determines whether this instance is identical to another.
    /// </summary>
    /// <param name="obj">The object to compare against.</param>
    /// <returns>True if they are equal, otherwise false.</returns>
    public override bool Equals(object? obj) => obj is Tag other && other.TypeIndex == this.TypeIndex;

    /// <summary>
    /// Returns a simple hash code based on the value of the instance.
    /// </summary>
    /// <returns>The hash code.</returns>
    public override int GetHashCode() => HashCode.Combine(0x7a9, (int)this.TypeIndex);

    internal void WriteTo(Writer writer)
    {
        writer.Write((byte)0x00);
        writer.WriteVar(this.TypeIndex);
    }
}
