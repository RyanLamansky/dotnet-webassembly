using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;

namespace WebAssembly.Instructions;

/// <summary>i8x16 unsigned add with saturation.</summary>
public class Int8x16AddSaturateUnsigned : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16AddSaturateUnsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16AddSaturateUnsigned;

    /// <summary>Creates a new <see cref="Int8x16AddSaturateUnsigned"/> instance.</summary>
    public Int8x16AddSaturateUnsigned() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b)
    {
        if (Sse2.IsSupported)
            return Sse2.AddSaturate(a, b);
        if (AdvSimd.IsSupported)
            return AdvSimd.AddSaturate(a, b);

        Span<byte> r = stackalloc byte[16];
        for (var i = 0; i < 16; i++) { var v = a.GetElement(i) + b.GetElement(i); r[i] = v > 255 ? (byte)255 : (byte)v; }
        return Vector128.Create(r);
    }
}
