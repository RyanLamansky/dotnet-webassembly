using System;
using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Copy data within (or between) linear memory regions. Stack: [dst:i32] [src:i32] [len:i32] → []
/// </summary>
public class MemoryCopy : MiscellaneousInstruction
{
    /// <summary>Always <see cref="MiscellaneousOpCode.MemoryCopy"/>.</summary>
    public sealed override MiscellaneousOpCode MiscellaneousOpCode => MiscellaneousOpCode.MemoryCopy;

    /// <summary>
    /// The destination memory index (WebAssembly 3.0 multiple memories), encoded as a <c>varuint32</c>; 0 for the default memory.
    /// </summary>
    public uint DestinationMemoryIndex { get; set; }

    /// <summary>
    /// The source memory index (WebAssembly 3.0 multiple memories), encoded as a <c>varuint32</c>; 0 for the default memory.
    /// </summary>
    public uint SourceMemoryIndex { get; set; }

    /// <summary>Creates a new <see cref="MemoryCopy"/> instance.</summary>
    public MemoryCopy() { }

    internal MemoryCopy(Reader reader)
    {
        DestinationMemoryIndex = reader.ReadVarUInt32();
        SourceMemoryIndex = reader.ReadVarUInt32();
    }

    internal sealed override void WriteTo(Writer writer)
    {
        writer.Write((byte)OpCode);
        writer.Write((byte)MiscellaneousOpCode);
        writer.WriteVar(DestinationMemoryIndex);
        writer.WriteVar(SourceMemoryIndex);
    }

    /// <inheritdoc/>
    public override bool Equals(Instruction? other) =>
        other is MemoryCopy mc && mc.DestinationMemoryIndex == DestinationMemoryIndex && mc.SourceMemoryIndex == SourceMemoryIndex;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine((int)MiscellaneousOpCode, (int)DestinationMemoryIndex, (int)SourceMemoryIndex);

    internal sealed override void Compile(CompilationContext context)
    {
        // Resolve both memories up front so an out-of-range index traps with the spec "unknown memory" wording.
        var destinationMemory = context.ResolveMemory(this.DestinationMemoryIndex);
        var sourceMemory = context.ResolveMemory(this.SourceMemoryIndex);

        // Stack: dst src len → (nothing)
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int32); // len
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int32); // src
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int32); // dst

        // Save the arguments to locals, then pass them to the copy helper in order.
        var len = context.DeclareLocal(typeof(uint));
        var src = context.DeclareLocal(typeof(uint));
        var dst = context.DeclareLocal(typeof(uint));

        context.Emit(OpCodes.Stloc, len);
        context.Emit(OpCodes.Stloc, src);
        context.Emit(OpCodes.Stloc, dst);

        if (this.DestinationMemoryIndex == this.SourceMemoryIndex)
        {
            // Within a single memory: this.memory.Copy(dst, src, len)
            context.EmitLoadThis();
            context.Emit(OpCodes.Ldfld, destinationMemory);
            context.Emit(OpCodes.Ldloc, dst);
            context.Emit(OpCodes.Ldloc, src);
            context.Emit(OpCodes.Ldloc, len);
            context.Emit(OpCodes.Call, UnmanagedMemory.CopyMethod);
        }
        else
        {
            // Across two memories: UnmanagedMemory.CopyBetween(dstMem, dst, srcMem, src, len)
            context.EmitLoadThis();
            context.Emit(OpCodes.Ldfld, destinationMemory);
            context.Emit(OpCodes.Ldloc, dst);
            context.EmitLoadThis();
            context.Emit(OpCodes.Ldfld, sourceMemory);
            context.Emit(OpCodes.Ldloc, src);
            context.Emit(OpCodes.Ldloc, len);
            context.Emit(OpCodes.Call, UnmanagedMemory.CopyBetweenMethod);
        }
    }
}
