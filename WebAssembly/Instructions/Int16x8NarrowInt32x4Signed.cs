using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace WebAssembly.Instructions;

/// <summary>Narrow i32x4 to i16x8, signed with saturation.</summary>
public class Int16x8NarrowInt32x4Signed : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8NarrowInt32x4Signed"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8NarrowInt32x4Signed;

    /// <summary>Creates a new <see cref="Int16x8NarrowInt32x4Signed"/> instance.</summary>
    public Int16x8NarrowInt32x4Signed() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b)
    {
        if (Sse2.IsSupported)
            return Sse2.PackSignedSaturate(a.AsInt32(), b.AsInt32()).AsByte();

        Span<short> r = stackalloc short[8];
        for (var i = 0; i < 4; i++) { var v = a.AsInt32().GetElement(i); r[i] = v < -32768 ? (short)-32768 : v > 32767 ? (short)32767 : (short)v; }
        for (var i = 0; i < 4; i++) { var v = b.AsInt32().GetElement(i); r[4 + i] = v < -32768 ? (short)-32768 : v > 32767 ? (short)32767 : (short)v; }
        return Vector128.Create(r[0], r[1], r[2], r[3], r[4], r[5], r[6], r[7]).AsByte();
    }
}
