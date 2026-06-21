using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>f64x2 negate.</summary>
public class Float64x2Neg : SimdUnaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Float64x2Neg"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float64x2Neg;

    /// <summary>Creates a new <see cref="Float64x2Neg"/> instance.</summary>
    public Float64x2Neg() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a) => (-a.AsDouble()).AsByte();
}
