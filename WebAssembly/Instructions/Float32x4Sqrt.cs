using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>f32x4 square root.</summary>
public class Float32x4Sqrt : SimdUnaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Float32x4Sqrt"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float32x4Sqrt;

    /// <summary>Creates a new <see cref="Float32x4Sqrt"/> instance.</summary>
    public Float32x4Sqrt() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a) => Vector128.Sqrt(a.AsSingle()).AsByte();
}
