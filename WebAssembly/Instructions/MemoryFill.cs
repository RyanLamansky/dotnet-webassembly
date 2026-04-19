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

    /// <summary>Memory index (currently must be 0).</summary>
    public byte MemIdx { get; set; }

    /// <summary>Creates a new <see cref="MemoryFill"/> instance.</summary>
    public MemoryFill() { }

    internal MemoryFill(Reader reader)
    {
        MemIdx = reader.ReadVarUInt1();
    }

    internal sealed override void WriteTo(Writer writer)
    {
        writer.Write((byte)OpCode);
        writer.Write((byte)MiscellaneousOpCode);
        writer.Write(MemIdx);
    }

    /// <inheritdoc/>
    public override bool Equals(Instruction? other) =>
        other is MemoryFill mf && mf.MemIdx == MemIdx;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine((int)OpCode, (int)MiscellaneousOpCode, MemIdx);

    internal sealed override void Compile(CompilationContext context)
    {
        // Stack: dst val len → (nothing)
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int32);
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int32);
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int32);

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
