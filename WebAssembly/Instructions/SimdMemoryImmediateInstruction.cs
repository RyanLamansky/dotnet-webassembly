namespace WebAssembly.Instructions;

/// <summary>
/// Base class for SIMD instructions that carry a memory immediate (alignment flags and an offset), such as the
/// v128 loads and stores.
/// </summary>
public abstract class SimdMemoryImmediateInstruction : SimdInstruction
{
    /// <summary>
    /// The bit in the alignment/flags field that signals an explicit <see cref="MemoryIndex"/> immediate follows
    /// (WebAssembly 3.0 multiple-memories encoding).
    /// </summary>
    private const uint MemoryIndexFlag = 0x40;

    /// <summary>Alignment flags (log2 of the byte alignment).</summary>
    public uint Flags { get; set; }

    /// <summary>Byte offset added to the address operand.</summary>
    public uint Offset { get; set; }

    /// <summary>
    /// The linear memory targeted by this access (WebAssembly 3.0 multiple memories); 0 for the default memory.
    /// A non-zero value is encoded by setting bit 6 of the alignment/flags field, followed by the index as a
    /// <c>varuint32</c>; index 0 uses the pre-3.0 minimal encoding so existing modules round-trip unchanged.
    /// </summary>
    public uint MemoryIndex { get; set; }

    private protected SimdMemoryImmediateInstruction()
    {
    }

    private protected SimdMemoryImmediateInstruction(Reader reader)
    {
        var flags = reader.ReadVarUInt32();
        if ((flags & MemoryIndexFlag) != 0)
        {
            this.MemoryIndex = reader.ReadVarUInt32();
            flags &= ~MemoryIndexFlag;
        }
        Flags = flags;
        Offset = reader.ReadVarUInt32();
    }

    internal override void WriteTo(Writer writer)
    {
        base.WriteTo(writer);
        if (this.MemoryIndex != 0)
        {
            writer.WriteVar(Flags | MemoryIndexFlag);
            writer.WriteVar(MemoryIndex);
        }
        else
        {
            writer.WriteVar(Flags);
        }
        writer.WriteVar(Offset);
    }

    /// <inheritdoc/>
    public override bool Equals(Instruction? other) =>
        other is SimdMemoryImmediateInstruction instruction
        && instruction.SimdOpCode == this.SimdOpCode
        && instruction.Flags == this.Flags
        && instruction.Offset == this.Offset
        && instruction.MemoryIndex == this.MemoryIndex;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine((int)this.SimdOpCode, (int)this.Flags, (int)this.Offset, (int)this.MemoryIndex);
}
