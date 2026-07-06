using System;
using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>
/// Relaxed i32x4 signed dot product of i8x16 and i7x16, with accumulator add.
/// The deterministic profile computes the i16x8 relaxed dot product (see
/// <see cref="Int16x8RelaxedDotInt8x16Int7x16Signed"/>), sign-extends and sums each adjacent i16 pair to
/// i32 (this add cannot overflow), then adds the i32x4 accumulator operand with wrapping.
/// </summary>
public class Int32x4RelaxedDotInt8x16Int7x16AddSigned : SimdTernaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int32x4RelaxedDotInt8x16Int7x16AddSigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int32x4RelaxedDotInt8x16Int7x16AddSigned;

    /// <summary>Creates a new <see cref="Int32x4RelaxedDotInt8x16Int7x16AddSigned"/> instance.</summary>
    public Int32x4RelaxedDotInt8x16Int7x16AddSigned() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b, Vector128<byte> c)
    {
        var dot = Int16x8RelaxedDotInt8x16Int7x16Signed.Execute(a, b).AsInt16();
        var ci = c.AsInt32();
        Span<int> r = stackalloc int[4];
        for (var k = 0; k < 4; k++)
            r[k] = unchecked(dot.GetElement(2 * k) + dot.GetElement(2 * k + 1) + ci.GetElement(k));
        return Vector128.Create((ReadOnlySpan<int>)r).AsByte();
    }
}
