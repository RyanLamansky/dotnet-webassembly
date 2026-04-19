using System;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Fill a range of a table with a reference value. Stack: [dst:i32] [ref] [len:i32] → []
/// </summary>
public class TableFill : MiscellaneousInstruction
{
    /// <summary>Always <see cref="MiscellaneousOpCode.TableFill"/>.</summary>
    public sealed override MiscellaneousOpCode MiscellaneousOpCode => MiscellaneousOpCode.TableFill;

    /// <summary>Table index.</summary>
    public uint TableIndex { get; set; }

    /// <summary>Creates a new <see cref="TableFill"/> instance.</summary>
    public TableFill() { }

    internal TableFill(Reader reader)
    {
        TableIndex = reader.ReadVarUInt32();
    }

    internal sealed override void WriteTo(Writer writer)
    {
        writer.Write((byte)OpCode);
        writer.Write((byte)MiscellaneousOpCode);
        writer.WriteVar(TableIndex);
    }

    /// <inheritdoc/>
    public override bool Equals(Instruction? other) =>
        other is TableFill tf && tf.TableIndex == TableIndex;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine((int)OpCode, (int)MiscellaneousOpCode, (int)TableIndex);

    internal sealed override void Compile(CompilationContext context)
    {
        throw new NotSupportedException("table.fill is not yet supported.");
    }
}
