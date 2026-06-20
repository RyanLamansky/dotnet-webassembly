using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>Load 8 bytes, zero-extend each byte to i16, producing an i16x8 v128.</summary>
public class V128Load8X8Unsigned : SimdMemoryImmediateInstruction
{
    /// <summary>Always <see cref="SimdOpCode.V128Load8X8Unsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.V128Load8X8Unsigned;

    /// <summary>Creates a new <see cref="V128Load8X8Unsigned"/> instance.</summary>
    public V128Load8X8Unsigned() { }

    internal V128Load8X8Unsigned(Reader reader)
        : base(reader)
    {
    }

    internal override void Compile(CompilationContext context)
    {
        if (this.Flags > 3)
            throw new Runtime.CompilerException("alignment must not be larger than natural");
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int32);

        if (this.Offset != 0)
        {
            Int32Constant.Emit(context, (int)this.Offset);
            context.Emit(OpCodes.Add_Ovf_Un);
        }

        context.EmitLoadThis();
        context.Emit(OpCodes.Call, context[HelperMethod.RangeCheck64, MemoryImmediateInstruction.CreateRangeCheck]);

        context.EmitLoadThis();
        context.Emit(OpCodes.Ldfld, context.CheckedMemory);
        context.Emit(OpCodes.Call, UnmanagedMemory.StartGetter);
        context.Emit(OpCodes.Add);

        context.Emit(OpCodes.Call, V128Helper.V128Load8x8UMethod.Reference);

        context.Stack.Push(WebAssemblyValueType.V128);
    }
}
