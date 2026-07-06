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

        var dst64 = context.IsMemory64(this.DestinationMemoryIndex);
        var src64 = context.IsMemory64(this.SourceMemoryIndex);
        // When either memory is 64-bit the whole operation runs through the u64 (memory64) path; a 32-bit side's
        // operand is widened to u64. The length operand's type is the narrower of the two memories (per the spec):
        // i64 only when both are 64-bit, else i32.
        var useUlong = dst64 || src64;
        var dstType = context.MemoryAddressType(this.DestinationMemoryIndex);
        var srcType = context.MemoryAddressType(this.SourceMemoryIndex);
        var lenType = dst64 && src64 ? WebAssemblyValueType.Int64 : WebAssemblyValueType.Int32;

        // Stack: dst src len → (nothing)
        context.PopStackNoReturn(this.OpCode, lenType); // len
        context.PopStackNoReturn(this.OpCode, srcType); // src
        context.PopStackNoReturn(this.OpCode, dstType); // dst

        // Save the arguments to locals, then pass them to the copy helper in order. Widen any i32 operand to u64
        // (zero-extending) so the memory64 overloads' overflow-checked bounds tests see the true value.
        var localType = useUlong ? typeof(ulong) : typeof(uint);
        var len = context.DeclareLocal(localType);
        var src = context.DeclareLocal(localType);
        var dst = context.DeclareLocal(localType);

        if (useUlong && lenType == WebAssemblyValueType.Int32)
            context.Emit(OpCodes.Conv_U8);
        context.Emit(OpCodes.Stloc, len);
        if (useUlong && srcType == WebAssemblyValueType.Int32)
            context.Emit(OpCodes.Conv_U8);
        context.Emit(OpCodes.Stloc, src);
        if (useUlong && dstType == WebAssemblyValueType.Int32)
            context.Emit(OpCodes.Conv_U8);
        context.Emit(OpCodes.Stloc, dst);

        if (this.DestinationMemoryIndex == this.SourceMemoryIndex)
        {
            // Within a single memory: this.memory.Copy(dst, src, len)
            context.EmitLoadThis();
            context.Emit(OpCodes.Ldfld, destinationMemory);
            context.Emit(OpCodes.Ldloc, dst);
            context.Emit(OpCodes.Ldloc, src);
            context.Emit(OpCodes.Ldloc, len);
            context.Emit(OpCodes.Call, useUlong ? UnmanagedMemory.Copy64Method : UnmanagedMemory.CopyMethod);
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
            context.Emit(OpCodes.Call, useUlong ? UnmanagedMemory.CopyBetween64Method : UnmanagedMemory.CopyBetweenMethod);
        }
    }
}
