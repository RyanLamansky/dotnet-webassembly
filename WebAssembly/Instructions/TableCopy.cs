using System;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Copy from one region of a table to another region
/// </summary>
public sealed class TableCopy : MiscellaneousInstruction
{
    /// <summary>
    /// Creates a new  <see cref="TableCopy"/> instance.
    /// </summary>
    public TableCopy() { }

    internal TableCopy(Reader reader)
    {
        reader.ReadByte(); // table_dst:0x00 table_src:0x00
        reader.ReadByte();
    }

    /// <inheritdoc/>
    public override MiscellaneousOpCode MiscellaneousOpCode => MiscellaneousOpCode.TableCopy;

    internal override void WriteTo(Writer writer)
    {
        base.WriteTo(writer);
        writer.Write(0);
        writer.Write(0);    
    }

    internal override void Compile(CompilationContext context)
    {
        throw new NotSupportedException();
    }
}
