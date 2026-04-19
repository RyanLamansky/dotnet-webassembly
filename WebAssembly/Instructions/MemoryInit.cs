using System;
using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Copy from a passive data segment into linear memory. Stack: [dst:i32] [src:i32] [len:i32] → []
/// </summary>
public class MemoryInit : MiscellaneousInstruction
{
    /// <summary>Always <see cref="MiscellaneousOpCode.MemoryInit"/>.</summary>
    public sealed override MiscellaneousOpCode MiscellaneousOpCode => MiscellaneousOpCode.MemoryInit;

    /// <summary>Data segment index.</summary>
    public uint SegmentIndex { get; set; }

    /// <summary>Memory index (currently must be 0).</summary>
    public byte MemIdx { get; set; }

    /// <summary>Creates a new <see cref="MemoryInit"/> instance.</summary>
    public MemoryInit() { }

    internal MemoryInit(Reader reader)
    {
        SegmentIndex = reader.ReadVarUInt32();
        MemIdx = reader.ReadVarUInt1();
    }

    internal sealed override void WriteTo(Writer writer)
    {
        writer.Write((byte)OpCode);
        writer.Write((byte)MiscellaneousOpCode);
        writer.WriteVar(SegmentIndex);
        writer.Write(MemIdx);
    }

    /// <inheritdoc/>
    public override bool Equals(Instruction? other) =>
        other is MemoryInit mi && mi.SegmentIndex == SegmentIndex && mi.MemIdx == MemIdx;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(HashCode.Combine((int)OpCode, (int)MiscellaneousOpCode), HashCode.Combine((int)SegmentIndex, MemIdx));

    internal sealed override void Compile(CompilationContext context)
    {
        // Stack: dst src_offset len → (nothing)
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int32); // len
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int32); // src offset
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int32); // dst

        if (!context.DataSegments.TryGetValue(SegmentIndex, out var segField))
            throw new CompilerException($"memory.init: data segment {SegmentIndex} is not a passive segment or does not exist.");

        // IL stack on entry: [..., dst:i32, srcOffset:i32, len:i32]  (len on top)
        var len = context.DeclareLocal(typeof(uint));
        var srcOffset = context.DeclareLocal(typeof(uint));
        var dst = context.DeclareLocal(typeof(uint));

        context.Emit(OpCodes.Stloc, len);
        context.Emit(OpCodes.Stloc, srcOffset);
        context.Emit(OpCodes.Stloc, dst);

        // Call: this.memory.InitFromSegment(dst, this.segField, srcOffset, len)
        context.EmitLoadThis();
        context.Emit(OpCodes.Ldfld, context.CheckedMemory);
        context.Emit(OpCodes.Ldloc, dst);
        context.EmitLoadThis();
        context.Emit(OpCodes.Ldfld, segField);
        context.Emit(OpCodes.Ldloc, srcOffset);
        context.Emit(OpCodes.Ldloc, len);
        context.Emit(OpCodes.Call, UnmanagedMemory.InitFromSegmentMethod);
    }
}
