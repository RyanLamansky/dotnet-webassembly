using System;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Copy from a passive element segment into a table. Stack: [dst:i32] [src:i32] [len:i32] → []
/// </summary>
public class TableInit : MiscellaneousInstruction
{
    /// <summary>Always <see cref="MiscellaneousOpCode.TableInit"/>.</summary>
    public sealed override MiscellaneousOpCode MiscellaneousOpCode => MiscellaneousOpCode.TableInit;

    /// <summary>Element segment index.</summary>
    public uint SegmentIndex { get; set; }

    /// <summary>Table index.</summary>
    public uint TableIndex { get; set; }

    /// <summary>Creates a new <see cref="TableInit"/> instance.</summary>
    public TableInit() { }

    internal TableInit(Reader reader)
    {
        SegmentIndex = reader.ReadVarUInt32();
        TableIndex = reader.ReadVarUInt32();
    }

    internal sealed override void WriteTo(Writer writer)
    {
        writer.Write((byte)OpCode);
        writer.Write((byte)MiscellaneousOpCode);
        writer.WriteVar(SegmentIndex);
        writer.WriteVar(TableIndex);
    }

    /// <inheritdoc/>
    public override bool Equals(Instruction? other) =>
        other is TableInit ti && ti.SegmentIndex == SegmentIndex && ti.TableIndex == TableIndex;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(HashCode.Combine((int)OpCode, (int)MiscellaneousOpCode), HashCode.Combine((int)SegmentIndex, (int)TableIndex));

    internal sealed override void Compile(CompilationContext context)
    {
        throw new NotSupportedException("table.init is not yet supported.");
    }
}
