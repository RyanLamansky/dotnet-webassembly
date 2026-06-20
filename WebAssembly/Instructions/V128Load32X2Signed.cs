using System;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>Load 8 bytes, sign-extend each i32 to i64, producing an i64x2 v128.</summary>
public class V128Load32X2Signed : SimdMemoryImmediateInstruction
{
    /// <summary>Always <see cref="SimdOpCode.V128Load32X2Signed"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.V128Load32X2Signed;

    /// <summary>Creates a new <see cref="V128Load32X2Signed"/> instance.</summary>
    public V128Load32X2Signed() { }

    internal V128Load32X2Signed(Reader reader)
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

        context.Emit(OpCodes.Call, ExecuteMethod(this.GetType()));

        context.Stack.Push(WebAssemblyValueType.V128);
    }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static unsafe Vector128<byte> Execute(IntPtr ptr)
    {
        if (Sse2.IsSupported)
        {
            var lanes = Vector128.CreateScalar(Unsafe.ReadUnaligned<ulong>((void*)ptr)).AsInt32();
            var sign = Sse2.CompareGreaterThan(Vector128<int>.Zero, lanes);
            return Sse2.UnpackLow(lanes, sign).AsByte();
        }

        var p = (byte*)ptr;
        Span<long> r = stackalloc long[2];
        for (var i = 0; i < 2; i++) r[i] = (int)(p[i * 4] | (p[i * 4 + 1] << 8) | (p[i * 4 + 2] << 16) | (p[i * 4 + 3] << 24));
        return Vector128.Create(r[0], r[1]).AsByte();
    }
}
