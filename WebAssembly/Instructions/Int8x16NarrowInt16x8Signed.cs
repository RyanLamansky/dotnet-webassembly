using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace WebAssembly.Instructions;

/// <summary>Narrow i16x8 to i8x16, signed with saturation.</summary>
public class Int8x16NarrowInt16x8Signed : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16NarrowInt16x8Signed"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16NarrowInt16x8Signed;

    /// <summary>Creates a new <see cref="Int8x16NarrowInt16x8Signed"/> instance.</summary>
    public Int8x16NarrowInt16x8Signed() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b)
    {
        if (Sse2.IsSupported)
            return Sse2.PackSignedSaturate(a.AsInt16(), b.AsInt16()).AsByte();

        Span<sbyte> r = stackalloc sbyte[16];
        for (var i = 0; i < 8; i++) { var v = a.AsInt16().GetElement(i); r[i] = v < -128 ? (sbyte)-128 : v > 127 ? (sbyte)127 : (sbyte)v; }
        for (var i = 0; i < 8; i++) { var v = b.AsInt16().GetElement(i); r[8 + i] = v < -128 ? (sbyte)-128 : v > 127 ? (sbyte)127 : (sbyte)v; }
        return Vector128.Create(
            r[0], r[1], r[2], r[3], r[4], r[5], r[6], r[7],
            r[8], r[9], r[10], r[11], r[12], r[13], r[14], r[15]).AsByte();
    }
}
