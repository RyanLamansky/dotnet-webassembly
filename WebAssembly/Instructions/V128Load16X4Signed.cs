using System;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>Load 8 bytes, sign-extend each i16 to i32, producing an i32x4 v128.</summary>
public class V128Load16X4Signed : SimdMemoryImmediateInstruction
{
    /// <summary>Always <see cref="SimdOpCode.V128Load16X4Signed"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.V128Load16X4Signed;

    /// <summary>Creates a new <see cref="V128Load16X4Signed"/> instance.</summary>
    public V128Load16X4Signed() { }

    internal V128Load16X4Signed(Reader reader)
        : base(reader)
    {
    }

    internal override void Compile(CompilationContext context)
    {
        if (this.Flags > 3)
            throw new Runtime.CompilerException("alignment must not be larger than natural");
        this.ValidateOffset(context);
        context.PopStackNoReturn(this.OpCode, context.MemoryAddressType(this.MemoryIndex));

        this.EmitAddressComputation(context, 8);

        context.Emit(OpCodes.Call, ExecuteMethod(this.GetType()));

        context.Stack.Push(WebAssemblyValueType.V128);
    }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static unsafe Vector128<byte> Execute(IntPtr ptr)
    {
        if (Sse2.IsSupported)
        {
            var lanes = Vector128.CreateScalar(Unsafe.ReadUnaligned<ulong>((void*)ptr)).AsInt16();
            var sign = Sse2.CompareGreaterThan(Vector128<short>.Zero, lanes);
            return Sse2.UnpackLow(lanes, sign).AsByte();
        }

        var p = (byte*)ptr;
        Span<int> r = stackalloc int[4];
        for (var i = 0; i < 4; i++) r[i] = (short)(p[i * 2] | (p[i * 2 + 1] << 8));
        return Vector128.Create(r[0], r[1], r[2], r[3]).AsByte();
    }
}
