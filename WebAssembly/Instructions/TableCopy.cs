using System;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Copy entries within or between tables. Stack: [dst:i32] [src:i32] [len:i32] → []
/// </summary>
public class TableCopy : MiscellaneousInstruction
{
    /// <summary>Always <see cref="MiscellaneousOpCode.TableCopy"/>.</summary>
    public sealed override MiscellaneousOpCode MiscellaneousOpCode => MiscellaneousOpCode.TableCopy;

    /// <summary>Destination table index.</summary>
    public uint DstTableIndex { get; set; }

    /// <summary>Source table index.</summary>
    public uint SrcTableIndex { get; set; }

    /// <summary>Creates a new <see cref="TableCopy"/> instance.</summary>
    public TableCopy() { }

    internal TableCopy(Reader reader)
    {
        DstTableIndex = reader.ReadVarUInt32();
        SrcTableIndex = reader.ReadVarUInt32();
    }

    internal sealed override void WriteTo(Writer writer)
    {
        writer.Write((byte)OpCode);
        writer.Write((byte)MiscellaneousOpCode);
        writer.WriteVar(DstTableIndex);
        writer.WriteVar(SrcTableIndex);
    }

    /// <inheritdoc/>
    public override bool Equals(Instruction? other) =>
        other is TableCopy tc && tc.DstTableIndex == DstTableIndex && tc.SrcTableIndex == SrcTableIndex;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(HashCode.Combine((int)OpCode, (int)MiscellaneousOpCode), HashCode.Combine((int)DstTableIndex, (int)SrcTableIndex));

    internal sealed override void Compile(CompilationContext context)
    {
        throw new NotSupportedException("table.copy is not yet supported.");
    }
}
