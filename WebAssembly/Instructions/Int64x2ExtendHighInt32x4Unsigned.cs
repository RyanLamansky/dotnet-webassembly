using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace WebAssembly.Instructions;

/// <summary>Int64x2ExtendHighInt32x4Unsigned instruction.</summary>
public class Int64x2ExtendHighInt32x4Unsigned : SimdUnaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int64x2ExtendHighInt32x4Unsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int64x2ExtendHighInt32x4Unsigned;

    /// <summary>Creates a new <see cref="Int64x2ExtendHighInt32x4Unsigned"/> instance.</summary>
    public Int64x2ExtendHighInt32x4Unsigned() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a)
    {
        if (Sse2.IsSupported)
            return Sse2.UnpackHigh(a.AsUInt32(), Vector128<uint>.Zero).AsByte();

        Span<ulong> r = stackalloc ulong[2];
        for (var i = 0; i < 2; i++) r[i] = a.AsUInt32().GetElement(2 + i);
        return Vector128.Create(r[0], r[1]).AsByte();
    }
}
