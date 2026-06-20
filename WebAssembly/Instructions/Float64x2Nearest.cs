using System;
using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Float64x2Nearest instruction.</summary>
public class Float64x2Nearest : SimdUnaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Float64x2Nearest"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float64x2Nearest;

    /// <summary>Creates a new <see cref="Float64x2Nearest"/> instance.</summary>
    public Float64x2Nearest() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a)
    {
        var r = new double[2];
        for (var i = 0; i < 2; i++) r[i] = Math.Round(a.AsDouble().GetElement(i), MidpointRounding.ToEven);
        return Vector128.Create(r).AsByte();
    }
}
