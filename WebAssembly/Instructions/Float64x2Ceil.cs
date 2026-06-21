using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>f64x2 ceiling.</summary>
public class Float64x2Ceil : SimdUnaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Float64x2Ceil"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float64x2Ceil;

    /// <summary>Creates a new <see cref="Float64x2Ceil"/> instance.</summary>
    public Float64x2Ceil() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a) => Vector128.Ceiling(a.AsDouble()).AsByte();
}
