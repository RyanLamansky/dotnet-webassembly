using System.Runtime.Intrinsics;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>f64x2 min.</summary>
public class Float64x2Min : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Float64x2Min"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float64x2Min;

    /// <summary>Creates a new <see cref="Float64x2Min"/> instance.</summary>
    public Float64x2Min() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b)
    {
#if NET9_0_OR_GREATER
        return Vector128.Min(a.AsDouble(), b.AsDouble()).AsByte();
#else
        var r = new double[2];
        var sa = a.AsDouble(); var sb = b.AsDouble();
        for (var i = 0; i < 2; i++)
        {
            var ai = sa.GetElement(i); var bi = sb.GetElement(i);
            double ri;
            if (double.IsNaN(ai) || double.IsNaN(bi)) ri = double.NaN;
            else if (ai == 0 && bi == 0) ri = FloatHelper.UInt64BitsToDouble(FloatHelper.DoubleToUInt64Bits(ai) | FloatHelper.DoubleToUInt64Bits(bi));
            else ri = ai < bi ? ai : bi;
            r[i] = ri;
        }
        return Vector128.Create(r).AsByte();
#endif
    }
}
