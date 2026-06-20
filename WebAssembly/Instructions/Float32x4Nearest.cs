using System;
using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Float32x4Nearest instruction.</summary>
public class Float32x4Nearest : SimdUnaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Float32x4Nearest"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float32x4Nearest;

    /// <summary>Creates a new <see cref="Float32x4Nearest"/> instance.</summary>
    public Float32x4Nearest() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a)
    {
        var r = new float[4];
        for (var i = 0; i < 4; i++) r[i] = MathF.Round(a.AsSingle().GetElement(i), MidpointRounding.ToEven);
        return Vector128.Create(r).AsByte();
    }
}
