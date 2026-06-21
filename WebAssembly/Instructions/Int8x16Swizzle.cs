using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace WebAssembly.Instructions;

/// <summary>Swizzle i8x16 lanes according to an index vector.</summary>
public class Int8x16Swizzle : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16Swizzle"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16Swizzle;

    /// <summary>Creates a new <see cref="Int8x16Swizzle"/> instance.</summary>
    public Int8x16Swizzle() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b)
    {
        if (Ssse3.IsSupported)
        {
            var highBit = Vector128.Create((byte)0x80);
            var compareBase = Vector128.Create(unchecked((sbyte)(0x80 | 0x0F)));
            var invalid = Sse2.CompareGreaterThan(Sse2.Xor(b, highBit).AsSByte(), compareBase);
            var mask = Sse2.Or(
                Sse2.AndNot(invalid.AsByte(), b),
                Sse2.And(invalid.AsByte(), highBit));
            return Ssse3.Shuffle(a, mask);
        }

        Span<byte> result = stackalloc byte[16];
        for (var i = 0; i < 16; i++)
        {
            var idx = b.GetElement(i);
            result[i] = idx < 16 ? a.GetElement(idx) : (byte)0;
        }

        return Vector128.Create(
            result[0], result[1], result[2], result[3],
            result[4], result[5], result[6], result[7],
            result[8], result[9], result[10], result[11],
            result[12], result[13], result[14], result[15]);
    }
}
