using System;
using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Prevent further use of a passive data segment, freeing its backing storage. Stack: [] → []
/// </summary>
public class DataDrop : MiscellaneousInstruction
{
    /// <summary>Always <see cref="MiscellaneousOpCode.DataDrop"/>.</summary>
    public sealed override MiscellaneousOpCode MiscellaneousOpCode => MiscellaneousOpCode.DataDrop;

    /// <summary>Data segment index.</summary>
    public uint SegmentIndex { get; set; }

    /// <summary>Creates a new <see cref="DataDrop"/> instance.</summary>
    public DataDrop() { }

    internal DataDrop(Reader reader)
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
        other is DataDrop dd && dd.SegmentIndex == SegmentIndex;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine((int)OpCode, (int)MiscellaneousOpCode, (int)SegmentIndex);

    internal sealed override void Compile(CompilationContext context)
    {
        if (!context.DataSegments.TryGetValue(SegmentIndex, out var segField))
            throw new ModuleLoadException($"data.drop: data segment {SegmentIndex} is not a passive segment or does not exist.", 0);

        // Null out the field so a subsequent memory.init on a dropped segment traps.
        context.EmitLoadThis();
        context.Emit(OpCodes.Ldnull);
        context.Emit(OpCodes.Stfld, segField);
    }
}
