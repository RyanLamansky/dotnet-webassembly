using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>Load one byte from memory into the specified lane of a v128.</summary>
public class V128Load8Lane : SimdMemoryLaneInstruction
{
    /// <summary>Always <see cref="SimdOpCode.V128Load8Lane"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.V128Load8Lane;

    /// <summary>Creates a new <see cref="V128Load8Lane"/> instance.</summary>
    public V128Load8Lane() { }

    internal V128Load8Lane(Reader reader)
        : base(reader)
    {
    }

    internal override void Compile(CompilationContext context)
    {
        // Stack: [..., i32_addr, v128] — pop v128, then i32 address
        if (this.Flags > 0)
            throw new Runtime.CompilerException("alignment must not be larger than natural");
        if (this.LaneIndex >= 16)
            throw new Runtime.CompilerException($"Lane index {LaneIndex} is out of range for V128Load8Lane (max 15).");
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.V128, WebAssemblyValueType.Int32);

        var vecLocal = context.DeclareLocal(V128Helper.V128Type);
        context.Emit(OpCodes.Stloc, vecLocal);

        if (this.Offset != 0)
        {
            Int32Constant.Emit(context, (int)this.Offset);
            context.Emit(OpCodes.Add_Ovf_Un);
        }

        context.EmitLoadThis();
        context.Emit(OpCodes.Call, context[HelperMethod.RangeCheck8, MemoryImmediateInstruction.CreateRangeCheck]);

        context.EmitLoadThis();
        context.Emit(OpCodes.Ldfld, context.CheckedMemory);
        context.Emit(OpCodes.Call, UnmanagedMemory.StartGetter);
        context.Emit(OpCodes.Add);

        context.Emit(OpCodes.Ldloc, vecLocal);
        context.Emit(OpCodes.Ldc_I4, (int)LaneIndex);
        context.Emit(OpCodes.Call, V128Helper.V128Load8LaneMethod.Reference);

        context.Stack.Push(WebAssemblyValueType.V128);
    }
}
