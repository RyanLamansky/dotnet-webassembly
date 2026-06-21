using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Convert unsigned i32x4 (low) to f64x2.</summary>
public class Float64x2ConvertLowInt32x4Unsigned : SimdUnaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Float64x2ConvertLowInt32x4Unsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float64x2ConvertLowInt32x4Unsigned;

    /// <summary>Creates a new <see cref="Float64x2ConvertLowInt32x4Unsigned"/> instance.</summary>
    public Float64x2ConvertLowInt32x4Unsigned() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a) { var r = new double[2]; for (var i = 0; i < 2; i++) r[i] = a.AsUInt32().GetElement(i); return Vector128.Create(r).AsByte(); }
}
