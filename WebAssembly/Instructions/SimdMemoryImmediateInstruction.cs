namespace WebAssembly.Instructions;

/// <summary>
/// Base class for SIMD instructions that carry a memory immediate (alignment flags and an offset), such as the
/// v128 loads and stores.
/// </summary>
public abstract class SimdMemoryImmediateInstruction : SimdInstruction
{
    /// <summary>Alignment flags (log2 of the byte alignment).</summary>
    public uint Flags { get; set; }

    /// <summary>Byte offset added to the address operand.</summary>
    public uint Offset { get; set; }

    private protected SimdMemoryImmediateInstruction()
    {
    }

    private protected SimdMemoryImmediateInstruction(Reader reader)
    {
        Flags = reader.ReadVarUInt32();
        Offset = reader.ReadVarUInt32();
    }

    internal override void WriteTo(Writer writer)
    {
        base.WriteTo(writer);
        writer.WriteVar(Flags);
        writer.WriteVar(Offset);
    }

    /// <inheritdoc/>
    public override bool Equals(Instruction? other) =>
        other is SimdMemoryImmediateInstruction instruction
        && instruction.SimdOpCode == this.SimdOpCode
        && instruction.Flags == this.Flags
        && instruction.Offset == this.Offset;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine((int)this.SimdOpCode, (int)this.Flags, (int)this.Offset);
}
