using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;

namespace WebAssembly.Instructions;

/// <summary>i8x16 unsigned subtract with saturation.</summary>
public class Int8x16SubSaturateUnsigned : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16SubSaturateUnsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16SubSaturateUnsigned;

    /// <summary>Creates a new <see cref="Int8x16SubSaturateUnsigned"/> instance.</summary>
    public Int8x16SubSaturateUnsigned() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b)
    {
        if (Sse2.IsSupported)
            return Sse2.SubtractSaturate(a, b);
        if (AdvSimd.IsSupported)
            return AdvSimd.SubtractSaturate(a, b);

        Span<byte> r = stackalloc byte[16];
        for (var i = 0; i < 16; i++) { var x = a.GetElement(i); var y = b.GetElement(i); r[i] = x < y ? (byte)0 : (byte)(x - y); }
        return Vector128.Create(r);
    }
}
