using System;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>Load 8 bytes, sign-extend each byte to i16, producing an i16x8 v128.</summary>
public class V128Load8X8Signed : SimdMemoryImmediateInstruction
{
    /// <summary>Always <see cref="SimdOpCode.V128Load8X8Signed"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.V128Load8X8Signed;

    /// <summary>Creates a new <see cref="V128Load8X8Signed"/> instance.</summary>
    public V128Load8X8Signed() { }

    internal V128Load8X8Signed(Reader reader)
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
            var lanes = Vector128.CreateScalar(Unsafe.ReadUnaligned<ulong>((void*)ptr)).AsByte();
            var sign = Sse2.CompareGreaterThan(Vector128<sbyte>.Zero, lanes.AsSByte()).AsByte();
            return Sse2.UnpackLow(lanes, sign).AsByte();
        }

        var p = (byte*)ptr;
        Span<short> r = stackalloc short[8];
        for (var i = 0; i < 8; i++) r[i] = (sbyte)p[i];
        return Vector128.Create(r[0], r[1], r[2], r[3], r[4], r[5], r[6], r[7]).AsByte();
    }
}
