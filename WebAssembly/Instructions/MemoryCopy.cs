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
    /// The destination memory index, encoded as a <c>varuint32</c>. The object model is permissive; the compiler
    /// requires 0 because multiple memories are not supported.
    /// </summary>
    public uint DestinationMemoryIndex { get; set; }

    /// <summary>
    /// The source memory index, encoded as a <c>varuint32</c>. The object model is permissive; the compiler
    /// requires 0 because multiple memories are not supported.
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
        if (DestinationMemoryIndex != 0 || SourceMemoryIndex != 0)
            throw new ModuleLoadException($"memory.copy: only memory index 0 is supported, found destination {DestinationMemoryIndex} and source {SourceMemoryIndex}.", 0);

        // Stack: dst src len → (nothing)
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int32); // len
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int32); // src
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int32); // dst

        // Save the arguments to locals, then pass them to UnmanagedMemory.Copy in order.
        var len = context.DeclareLocal(typeof(uint));
        var src = context.DeclareLocal(typeof(uint));
        var dst = context.DeclareLocal(typeof(uint));

        context.Emit(OpCodes.Stloc, len);
        context.Emit(OpCodes.Stloc, src);
        context.Emit(OpCodes.Stloc, dst);

        context.EmitLoadThis();
        context.Emit(OpCodes.Ldfld, context.CheckedMemory);
        context.Emit(OpCodes.Ldloc, dst);
        context.Emit(OpCodes.Ldloc, src);
        context.Emit(OpCodes.Ldloc, len);
        context.Emit(OpCodes.Call, UnmanagedMemory.CopyMethod);
    }
}
