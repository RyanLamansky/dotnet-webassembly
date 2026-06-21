using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace WebAssembly.Instructions;

/// <summary>Widen low i8x16 lanes to i16x8, signed.</summary>
public class Int16x8ExtendLowInt8x16Signed : SimdUnaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8ExtendLowInt8x16Signed"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8ExtendLowInt8x16Signed;

    /// <summary>Creates a new <see cref="Int16x8ExtendLowInt8x16Signed"/> instance.</summary>
    public Int16x8ExtendLowInt8x16Signed() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a)
    {
        if (Sse2.IsSupported)
        {
            var sign = Sse2.CompareGreaterThan(Vector128<sbyte>.Zero, a.AsSByte()).AsByte();
            return Sse2.UnpackLow(a, sign).AsByte();
        }

        Span<short> r = stackalloc short[8];
        for (var i = 0; i < 8; i++) r[i] = (sbyte)a.GetElement(i);
        return Vector128.Create(r[0], r[1], r[2], r[3], r[4], r[5], r[6], r[7]).AsByte();
    }
}
