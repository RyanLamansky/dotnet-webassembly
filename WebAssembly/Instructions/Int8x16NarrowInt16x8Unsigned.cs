using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace WebAssembly.Instructions;

/// <summary>Int8x16NarrowInt16x8Unsigned instruction.</summary>
public class Int8x16NarrowInt16x8Unsigned : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16NarrowInt16x8Unsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16NarrowInt16x8Unsigned;

    /// <summary>Creates a new <see cref="Int8x16NarrowInt16x8Unsigned"/> instance.</summary>
    public Int8x16NarrowInt16x8Unsigned() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b)
    {
        if (Sse2.IsSupported)
            return Sse2.PackUnsignedSaturate(a.AsInt16(), b.AsInt16());

        Span<byte> r = stackalloc byte[16];
        for (var i = 0; i < 8; i++) { var v = a.AsInt16().GetElement(i); r[i] = v < 0 ? (byte)0 : v > 255 ? (byte)255 : (byte)v; }
        for (var i = 0; i < 8; i++) { var v = b.AsInt16().GetElement(i); r[8 + i] = v < 0 ? (byte)0 : v > 255 ? (byte)255 : (byte)v; }
        return Vector128.Create(
            r[0], r[1], r[2], r[3], r[4], r[5], r[6], r[7],
            r[8], r[9], r[10], r[11], r[12], r[13], r[14], r[15]);
    }
}
