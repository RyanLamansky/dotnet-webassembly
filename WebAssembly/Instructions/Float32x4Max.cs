using System.Runtime.Intrinsics;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>f32x4 max.</summary>
public class Float32x4Max : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Float32x4Max"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float32x4Max;

    /// <summary>Creates a new <see cref="Float32x4Max"/> instance.</summary>
    public Float32x4Max() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b)
    {
#if NET9_0_OR_GREATER
        return Vector128.Max(a.AsSingle(), b.AsSingle()).AsByte();
#else
        var r = new float[4];
        var sa = a.AsSingle(); var sb = b.AsSingle();
        for (var i = 0; i < 4; i++)
        {
            var ai = sa.GetElement(i); var bi = sb.GetElement(i);
            float ri;
            if (float.IsNaN(ai) || float.IsNaN(bi)) ri = float.NaN;
            else if (ai == 0 && bi == 0) ri = FloatHelper.UInt32BitsToFloat(FloatHelper.FloatToUInt32Bits(ai) & FloatHelper.FloatToUInt32Bits(bi));
            else ri = ai > bi ? ai : bi;
            r[i] = ri;
        }
        return Vector128.Create(r).AsByte();
#endif
    }
}
