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

    /// <summary>
    /// The target memory index (WebAssembly 3.0 multiple memories), encoded as a <c>varuint32</c>; 0 for the default memory.
    /// </summary>
    public uint MemoryIndex { get; set; }

    /// <summary>Creates a new <see cref="MemoryInit"/> instance.</summary>
    public MemoryInit() { }

    internal MemoryInit(Reader reader)
    {
        SegmentIndex = reader.ReadVarUInt32();
        MemoryIndex = reader.ReadVarUInt32();
    }

    internal sealed override void WriteTo(Writer writer)
    {
        writer.Write((byte)OpCode);
        writer.Write((byte)MiscellaneousOpCode);
        writer.WriteVar(SegmentIndex);
        writer.WriteVar(MemoryIndex);
    }

    /// <inheritdoc/>
    public override bool Equals(Instruction? other) =>
        other is MemoryInit mi && mi.SegmentIndex == SegmentIndex && mi.MemoryIndex == MemoryIndex;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine((int)MiscellaneousOpCode, (int)SegmentIndex, (int)MemoryIndex);

    internal sealed override void Compile(CompilationContext context)
    {
        // Resolve the target memory up front so an out-of-range index traps with the spec "unknown memory" wording.
        var memory = context.ResolveMemory(this.MemoryIndex);
        var is64 = context.IsMemory64(this.MemoryIndex);

        // Stack: dst src_offset len → (nothing). For memory64 the destination is i64; the source offset and
        // length remain i32 (the source is a data segment, which is always 32-bit indexed).
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int32); // len
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int32); // src offset
        context.PopStackNoReturn(this.OpCode, context.MemoryAddressType(this.MemoryIndex)); // dst

        if (!context.DataSegments.TryGetValue(SegmentIndex, out var segField))
            throw new ModuleLoadException($"memory.init: data segment {SegmentIndex} is not a passive segment or does not exist.", 0);

        // IL stack on entry: [..., dst, srcOffset:i32, len:i32]  (len on top)
        var len = context.DeclareLocal(typeof(uint));
        var srcOffset = context.DeclareLocal(typeof(uint));
        var dst = context.DeclareLocal(is64 ? typeof(ulong) : typeof(uint));

        context.Emit(OpCodes.Stloc, len);
        context.Emit(OpCodes.Stloc, srcOffset);
        context.Emit(OpCodes.Stloc, dst);

        // Call: this.memory.InitFromSegment(dst, this.segField, srcOffset, len)
        context.EmitLoadThis();
        context.Emit(OpCodes.Ldfld, memory);
        context.Emit(OpCodes.Ldloc, dst);
        context.EmitLoadThis();
        context.Emit(OpCodes.Ldfld, segField);
        context.Emit(OpCodes.Ldloc, srcOffset);
        context.Emit(OpCodes.Ldloc, len);
        context.Emit(OpCodes.Call, is64 ? UnmanagedMemory.InitFromSegment64Method : UnmanagedMemory.InitFromSegmentMethod);
    }
}
