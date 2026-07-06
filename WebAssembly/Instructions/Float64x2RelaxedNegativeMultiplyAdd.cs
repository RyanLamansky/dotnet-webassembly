using System;
using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>f64x2 fused negative multiply-add: each lane is <c>-(a*b) + c</c> with a single rounding.</summary>
public class Float64x2RelaxedNegativeMultiplyAdd : SimdTernaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Float64x2RelaxedNegativeMultiplyAdd"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float64x2RelaxedNegativeMultiplyAdd;

    /// <summary>Creates a new <see cref="Float64x2RelaxedNegativeMultiplyAdd"/> instance.</summary>
    public Float64x2RelaxedNegativeMultiplyAdd() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b, Vector128<byte> c)
    {
        var da = a.AsDouble();
        var db = b.AsDouble();
        var dc = c.AsDouble();
        Span<double> r = stackalloc double[2];
        for (var i = 0; i < 2; i++)
            r[i] = Math.FusedMultiplyAdd(-da.GetElement(i), db.GetElement(i), dc.GetElement(i));
        return Vector128.Create(r).AsByte();
    }
}
