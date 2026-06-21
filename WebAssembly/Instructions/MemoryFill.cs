using System;
using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Fill a region of linear memory with a byte value. Stack: [dst:i32] [val:i32] [len:i32] → []
/// </summary>
public class MemoryFill : MiscellaneousInstruction
{
    /// <summary>Always <see cref="MiscellaneousOpCode.MemoryFill"/>.</summary>
    public sealed override MiscellaneousOpCode MiscellaneousOpCode => MiscellaneousOpCode.MemoryFill;

    /// <summary>
    /// The target memory index, encoded as a <c>varuint32</c>. The object model is permissive; the compiler
    /// requires 0 because multiple memories are not supported.
    /// </summary>
    public uint MemoryIndex { get; set; }

    /// <summary>Creates a new <see cref="MemoryFill"/> instance.</summary>
    public MemoryFill() { }

    internal MemoryFill(Reader reader)
    {
        MemoryIndex = reader.ReadVarUInt32();
    }

    internal sealed override void WriteTo(Writer writer)
    {
        writer.Write((byte)OpCode);
        writer.Write((byte)MiscellaneousOpCode);
        writer.WriteVar(MemoryIndex);
    }

    /// <inheritdoc/>
    public override bool Equals(Instruction? other) =>
        other is MemoryFill mf && mf.MemoryIndex == MemoryIndex;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine((int)OpCode, (int)MiscellaneousOpCode, (int)MemoryIndex);

    internal sealed override void Compile(CompilationContext context)
    {
        if (MemoryIndex != 0)
            throw new ModuleLoadException($"memory.fill: only memory index 0 is supported, found {MemoryIndex}.", 0);

        // Stack: dst val len → (nothing)
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int32); // len
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int32); // val
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int32); // dst

        var len = context.DeclareLocal(typeof(uint));
        var val = context.DeclareLocal(typeof(uint));
        var dst = context.DeclareLocal(typeof(uint));

        context.Emit(OpCodes.Stloc, len);
        context.Emit(OpCodes.Stloc, val);
        context.Emit(OpCodes.Stloc, dst);

        context.EmitLoadThis();
        context.Emit(OpCodes.Ldfld, context.CheckedMemory);
        context.Emit(OpCodes.Ldloc, dst);
        context.Emit(OpCodes.Ldloc, val);
        context.Emit(OpCodes.Ldloc, len);
        context.Emit(OpCodes.Call, UnmanagedMemory.FillMethod);
    }
}
