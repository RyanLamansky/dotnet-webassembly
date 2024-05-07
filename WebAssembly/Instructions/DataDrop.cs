using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Prevent further use of passive data segment
/// </summary>
public sealed class DataDrop : MiscellaneousInstruction
{
    /// <summary>
    /// Creates a new  <see cref="DataDrop"/> instance.
    /// </summary>
    public DataDrop() { }

    /// <summary>
    /// Creates a new  <see cref="DataDrop"/> instance.
    /// </summary>
    public DataDrop(uint dataIdx) => DataIdx = dataIdx;

    internal DataDrop(Reader reader) => DataIdx = reader.ReadVarUInt32(); // segment:varuint32

    /// <inheritdoc/>
    public override MiscellaneousOpCode MiscellaneousOpCode => MiscellaneousOpCode.DataDrop;

    /// <summary>
    /// Passive data segment Idx
    /// </summary>
    public uint DataIdx;

    internal override void WriteTo(Writer writer)
    {
        base.WriteTo(writer);
        writer.WriteVar(DataIdx);
    }

    internal override void Compile(CompilationContext context)
    {
        // TODO: add trap
    }
}
