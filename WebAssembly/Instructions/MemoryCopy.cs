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

    /// <summary>Destination memory index (currently must be 0).</summary>
    public byte DestinationMemoryIndex { get; set; }

    /// <summary>Source memory index (currently must be 0).</summary>
    public byte SourceMemoryIndex { get; set; }

    /// <summary>Creates a new <see cref="MemoryCopy"/> instance.</summary>
    public MemoryCopy() { }

    internal MemoryCopy(Reader reader)
    {
        DestinationMemoryIndex = reader.ReadVarUInt1();
        SourceMemoryIndex = reader.ReadVarUInt1();
    }

    internal sealed override void WriteTo(Writer writer)
    {
        writer.Write((byte)OpCode);
        writer.Write((byte)MiscellaneousOpCode);
        writer.Write(DestinationMemoryIndex);
        writer.Write(SourceMemoryIndex);
    }

    /// <inheritdoc/>
    public override bool Equals(Instruction? other) =>
        other is MemoryCopy mc && mc.DestinationMemoryIndex == DestinationMemoryIndex && mc.SourceMemoryIndex == SourceMemoryIndex;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine((int)MiscellaneousOpCode, DestinationMemoryIndex, SourceMemoryIndex);

    internal sealed override void Compile(CompilationContext context)
    {
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
