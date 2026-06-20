using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace WebAssembly.Instructions;

/// <summary>Int32x4ExtendHighInt16x8Unsigned instruction.</summary>
public class Int32x4ExtendHighInt16x8Unsigned : SimdUnaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int32x4ExtendHighInt16x8Unsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int32x4ExtendHighInt16x8Unsigned;

    /// <summary>Creates a new <see cref="Int32x4ExtendHighInt16x8Unsigned"/> instance.</summary>
    public Int32x4ExtendHighInt16x8Unsigned() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a)
    {
        if (Sse2.IsSupported)
            return Sse2.UnpackHigh(a.AsUInt16(), Vector128<ushort>.Zero).AsByte();

        Span<uint> r = stackalloc uint[4];
        for (var i = 0; i < 4; i++) r[i] = a.AsUInt16().GetElement(4 + i);
        return Vector128.Create(r[0], r[1], r[2], r[3]).AsByte();
    }
}
