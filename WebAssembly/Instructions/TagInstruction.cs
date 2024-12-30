using System;

namespace WebAssembly.Instructions;

/// <summary>
/// An instruction that accesses a variable by its 0-based index.
/// </summary>
public abstract class TagInstruction : Instruction, IEquatable<TagInstruction>
{
    /// <summary>
    /// The 0-based index of the tag.
    /// </summary>
    public uint Index { get; set; }

    private protected TagInstruction(Reader reader)
    {
#if NETSTANDARD
        if (reader == null)
            throw new ArgumentNullException(nameof(reader));
#else
        ArgumentNullException.ThrowIfNull(reader, nameof(reader));
#endif

        Index = reader.ReadVarUInt32();
    }

    /// <summary>
    /// Creates a new <see cref="TagInstruction"/> for the provided tag index.
    /// </summary>
    /// <param name="index">The index of the tag.</param>
    private protected TagInstruction(uint index)
    {
        this.Index = index;
    }

    /// <summary>
    /// Creates a new <see cref="TagInstruction"/> instance.
    /// </summary>
    private protected TagInstruction()
    {
    }

    internal sealed override void WriteTo(Writer writer)
    {
        writer.Write((byte)this.OpCode);
        writer.WriteVar(this.Index);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is TagInstruction instruction
            && this.Equals(instruction);
    }

    /// <summary>
    /// Determines whether this instruction is identical to another.
    /// </summary>
    /// <param name="other">The instruction to compare against.</param>
    /// <returns>True if they have the same type and value, otherwise false.</returns>
    public override bool Equals(Instruction? other) =>
        this.Equals(other as TagInstruction);

    /// <summary>
    /// Determines whether this instruction is identical to another.
    /// </summary>
    /// <param name="other">The instruction to compare against.</param>
    /// <returns>True if they have the same type and value, otherwise false.</returns>
    public bool Equals(TagInstruction? other) =>
        other != null
        && other.OpCode == this.OpCode
        && other.Index == this.Index
        ;

    /// <summary>
    /// Returns a simple hash code based on the value of the instruction.
    /// </summary>
    /// <returns>The hash code.</returns>
    public override int GetHashCode() => HashCode.Combine((int)this.OpCode, this.Index.GetHashCode());

    /// <summary>
    /// Provides a native representation of the instruction and the variable index.
    /// </summary>
    /// <returns>A string representation of this instance and the variable index.</returns>
    public override string ToString() => $"{base.ToString()} {Index}";
}
