using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace WebAssembly.Instructions;

/// <summary>Narrow i32x4 to i16x8, unsigned with saturation.</summary>
public class Int16x8NarrowInt32x4Unsigned : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8NarrowInt32x4Unsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8NarrowInt32x4Unsigned;

    /// <summary>Creates a new <see cref="Int16x8NarrowInt32x4Unsigned"/> instance.</summary>
    public Int16x8NarrowInt32x4Unsigned() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b)
    {
        if (Sse41.IsSupported)
            return Sse41.PackUnsignedSaturate(a.AsInt32(), b.AsInt32()).AsByte();

        Span<ushort> r = stackalloc ushort[8];
        for (var i = 0; i < 4; i++) { var v = a.AsInt32().GetElement(i); r[i] = v < 0 ? (ushort)0 : v > 65535 ? (ushort)65535 : (ushort)v; }
        for (var i = 0; i < 4; i++) { var v = b.AsInt32().GetElement(i); r[4 + i] = v < 0 ? (ushort)0 : v > 65535 ? (ushort)65535 : (ushort)v; }
        return Vector128.Create(r[0], r[1], r[2], r[3], r[4], r[5], r[6], r[7]).AsByte();
    }
}
