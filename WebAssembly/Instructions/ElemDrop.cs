using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Prevent further use of passive data segment
/// </summary>
public sealed class ElemDrop : MiscellaneousInstruction
{
    /// <summary>
    /// Creates a new  <see cref="ElemDrop"/> instance.
    /// </summary>
    public ElemDrop() { }

    /// <summary>
    /// Creates a new  <see cref="ElemDrop"/> instance.
    /// </summary>
    public ElemDrop(uint elemIdx) => ElemIdx = elemIdx;

    internal ElemDrop(Reader reader) => ElemIdx = reader.ReadVarUInt32(); // segment:varuint32

    /// <inheritdoc/>
    public override MiscellaneousOpCode MiscellaneousOpCode => MiscellaneousOpCode.ElemDrop;

    /// <summary>
    /// Passive data segment Idx
    /// </summary>
    public uint ElemIdx;

    internal override void WriteTo(Writer writer)
    {
        base.WriteTo(writer);
        writer.WriteVar(ElemIdx);
    }

    internal override void Compile(CompilationContext context)
    {
        // TODO: add trap
    }
}
