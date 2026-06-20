using System;
using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Float64x2Trunc instruction.</summary>
public class Float64x2Trunc : SimdUnaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Float64x2Trunc"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float64x2Trunc;

    /// <summary>Creates a new <see cref="Float64x2Trunc"/> instance.</summary>
    public Float64x2Trunc() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a)
    {
        var r = new double[2];
        for (var i = 0; i < 2; i++) r[i] = Math.Truncate(a.AsDouble().GetElement(i));
        return Vector128.Create(r).AsByte();
    }
}
