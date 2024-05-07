using System;
using System.Diagnostics.Contracts;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Copy from a passive element segment to a table
/// </summary>
public sealed class TableInit : MiscellaneousInstruction
{
    /// <summary>
    /// Creates a new  <see cref="TableInit"/> instance.
    /// </summary>
    public TableInit() { }

    /// <summary>
    /// Creates a new  <see cref="TableInit"/> instance.
    /// </summary>
    public TableInit(uint segment) => Segment = segment;

    internal TableInit(Reader reader)
    {
        // segment:varuint32, table:0x00
        Segment = reader.ReadVarUInt32(); 
        reader.ReadByte();
    }

    /// <inheritdoc/>
    public override MiscellaneousOpCode MiscellaneousOpCode => MiscellaneousOpCode.TableInit;

    /// <summary>
    /// Passive element segment
    /// </summary>
    public uint Segment;

    internal override void WriteTo(Writer writer)
    {
        base.WriteTo(writer);
        writer.WriteVar(Segment);
        writer.Write(0);
    }

    internal override void Compile(CompilationContext context)
    {
        throw new NotSupportedException();
    }
}
