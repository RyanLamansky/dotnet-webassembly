using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>f32x4 absolute value.</summary>
public class Float32x4Abs : SimdUnaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Float32x4Abs"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float32x4Abs;

    /// <summary>Creates a new <see cref="Float32x4Abs"/> instance.</summary>
    public Float32x4Abs() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a) => Vector128.Abs(a.AsSingle()).AsByte();
}
