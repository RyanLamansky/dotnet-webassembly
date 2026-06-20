using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace WebAssembly.Instructions;

/// <summary>Int16x8ExtendHighInt8x16Unsigned instruction.</summary>
public class Int16x8ExtendHighInt8x16Unsigned : SimdUnaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8ExtendHighInt8x16Unsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8ExtendHighInt8x16Unsigned;

    /// <summary>Creates a new <see cref="Int16x8ExtendHighInt8x16Unsigned"/> instance.</summary>
    public Int16x8ExtendHighInt8x16Unsigned() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a)
    {
        if (Sse2.IsSupported)
            return Sse2.UnpackHigh(a, Vector128<byte>.Zero).AsByte();

        Span<ushort> r = stackalloc ushort[8];
        for (var i = 0; i < 8; i++) r[i] = a.GetElement(8 + i);
        return Vector128.Create(r[0], r[1], r[2], r[3], r[4], r[5], r[6], r[7]).AsByte();
    }
}
