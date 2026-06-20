using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace WebAssembly.Instructions;

/// <summary>Int32x4ExtendHighInt16x8Signed instruction.</summary>
public class Int32x4ExtendHighInt16x8Signed : SimdUnaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int32x4ExtendHighInt16x8Signed"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int32x4ExtendHighInt16x8Signed;

    /// <summary>Creates a new <see cref="Int32x4ExtendHighInt16x8Signed"/> instance.</summary>
    public Int32x4ExtendHighInt16x8Signed() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a)
    {
        if (Sse2.IsSupported)
        {
            var lanes = a.AsInt16();
            var sign = Sse2.CompareGreaterThan(Vector128<short>.Zero, lanes);
            return Sse2.UnpackHigh(lanes, sign).AsByte();
        }

        Span<int> r = stackalloc int[4];
        for (var i = 0; i < 4; i++) r[i] = a.AsInt16().GetElement(4 + i);
        return Vector128.Create(r[0], r[1], r[2], r[3]).AsByte();
    }
}
