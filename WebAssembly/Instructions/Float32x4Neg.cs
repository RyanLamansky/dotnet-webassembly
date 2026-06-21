using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>f32x4 negate.</summary>
public class Float32x4Neg : SimdUnaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Float32x4Neg"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float32x4Neg;

    /// <summary>Creates a new <see cref="Float32x4Neg"/> instance.</summary>
    public Float32x4Neg() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a) => (-a.AsSingle()).AsByte();
}
