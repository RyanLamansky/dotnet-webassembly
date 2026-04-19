using System;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Drop a passive element segment. Stack: [] → []
/// </summary>
public class ElemDrop : MiscellaneousInstruction
{
    /// <summary>Always <see cref="MiscellaneousOpCode.ElemDrop"/>.</summary>
    public sealed override MiscellaneousOpCode MiscellaneousOpCode => MiscellaneousOpCode.ElemDrop;

    /// <summary>Element segment index.</summary>
    public uint SegmentIndex { get; set; }

    /// <summary>Creates a new <see cref="ElemDrop"/> instance.</summary>
    public ElemDrop() { }

    internal ElemDrop(Reader reader)
    {
        SegmentIndex = reader.ReadVarUInt32();
    }

    internal sealed override void WriteTo(Writer writer)
    {
        writer.Write((byte)OpCode);
        writer.Write((byte)MiscellaneousOpCode);
        writer.WriteVar(SegmentIndex);
    }

    /// <inheritdoc/>
    public override bool Equals(Instruction? other) =>
        other is ElemDrop ed && ed.SegmentIndex == SegmentIndex;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine((int)OpCode, (int)MiscellaneousOpCode, (int)SegmentIndex);

    internal sealed override void Compile(CompilationContext context)
    {
        throw new NotSupportedException("elem.drop is not yet supported.");
    }
}
