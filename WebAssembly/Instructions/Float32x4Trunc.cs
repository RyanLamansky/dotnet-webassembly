using System;
using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Float32x4Trunc instruction.</summary>
public class Float32x4Trunc : SimdUnaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Float32x4Trunc"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float32x4Trunc;

    /// <summary>Creates a new <see cref="Float32x4Trunc"/> instance.</summary>
    public Float32x4Trunc() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a)
    {
        var r = new float[4];
        for (var i = 0; i < 4; i++) r[i] = MathF.Truncate(a.AsSingle().GetElement(i));
        return Vector128.Create(r).AsByte();
    }
}
