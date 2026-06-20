namespace WebAssembly.Instructions;

/// <summary>
/// Base class for SIMD load-lane and store-lane instructions, which add a lane index to the memory immediate.
/// </summary>
public abstract class SimdMemoryLaneInstruction : SimdMemoryImmediateInstruction
{
    /// <summary>The lane index read or written by the instruction.</summary>
    public byte LaneIndex { get; set; }

    private protected SimdMemoryLaneInstruction()
    {
    }

    private protected SimdMemoryLaneInstruction(Reader reader)
        : base(reader)
    {
        LaneIndex = reader.ReadByte();
    }

    internal override void WriteTo(Writer writer)
    {
        base.WriteTo(writer);
        writer.Write(LaneIndex);
    }

    /// <inheritdoc/>
    public override bool Equals(Instruction? other) =>
        other is SimdMemoryLaneInstruction instruction
        && instruction.SimdOpCode == this.SimdOpCode
        && instruction.Flags == this.Flags
        && instruction.Offset == this.Offset
        && instruction.LaneIndex == this.LaneIndex;

    /// <inheritdoc/>
    public override int GetHashCode() =>
        HashCode.Combine(HashCode.Combine((int)this.SimdOpCode, (int)this.Flags, (int)this.Offset), (int)this.LaneIndex);
}
