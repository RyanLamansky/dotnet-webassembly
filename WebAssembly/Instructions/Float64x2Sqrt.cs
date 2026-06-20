using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Float64x2Sqrt instruction.</summary>
public class Float64x2Sqrt : SimdUnaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Float64x2Sqrt"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float64x2Sqrt;

    /// <summary>Creates a new <see cref="Float64x2Sqrt"/> instance.</summary>
    public Float64x2Sqrt() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a) => Vector128.Sqrt(a.AsDouble()).AsByte();
}
