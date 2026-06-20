using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace WebAssembly.Instructions;

/// <summary>Int64x2ExtendHighInt32x4Signed instruction.</summary>
public class Int64x2ExtendHighInt32x4Signed : SimdUnaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int64x2ExtendHighInt32x4Signed"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int64x2ExtendHighInt32x4Signed;

    /// <summary>Creates a new <see cref="Int64x2ExtendHighInt32x4Signed"/> instance.</summary>
    public Int64x2ExtendHighInt32x4Signed() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a)
    {
        if (Sse2.IsSupported)
        {
            var lanes = a.AsInt32();
            var sign = Sse2.CompareGreaterThan(Vector128<int>.Zero, lanes);
            return Sse2.UnpackHigh(lanes, sign).AsByte();
        }

        Span<long> r = stackalloc long[2];
        for (var i = 0; i < 2; i++) r[i] = a.AsInt32().GetElement(2 + i);
        return Vector128.Create(r[0], r[1]).AsByte();
    }
}
