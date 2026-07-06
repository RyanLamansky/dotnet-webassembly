using System;
using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>
/// Relaxed i16x8 signed dot product of i8x16 and i7x16.
/// The deterministic profile interprets both operands as signed i8, forms the two products of each
/// adjacent lane pair, and combines them with signed-saturating i16 addition (WASM 3.0 <c>ivdotsat</c>,
/// R_idot = 0).
/// </summary>
public class Int16x8RelaxedDotInt8x16Int7x16Signed : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8RelaxedDotInt8x16Int7x16Signed"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8RelaxedDotInt8x16Int7x16Signed;

    /// <summary>Creates a new <see cref="Int16x8RelaxedDotInt8x16Int7x16Signed"/> instance.</summary>
    public Int16x8RelaxedDotInt8x16Int7x16Signed() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b)
    {
        var sa = a.AsSByte();
        var sb = b.AsSByte();
        Span<short> r = stackalloc short[8];
        for (var i = 0; i < 8; i++)
        {
            var sum = sa.GetElement(2 * i) * sb.GetElement(2 * i)
                + sa.GetElement(2 * i + 1) * sb.GetElement(2 * i + 1);
            r[i] = sum > 32767 ? (short)32767 : sum < -32768 ? (short)-32768 : (short)sum;
        }
        return Vector128.Create((ReadOnlySpan<short>)r).AsByte();
    }
}
