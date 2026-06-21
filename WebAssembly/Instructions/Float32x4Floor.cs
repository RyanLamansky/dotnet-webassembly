using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>f32x4 floor.</summary>
public class Float32x4Floor : SimdUnaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Float32x4Floor"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float32x4Floor;

    /// <summary>Creates a new <see cref="Float32x4Floor"/> instance.</summary>
    public Float32x4Floor() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a) => Vector128.Floor(a.AsSingle()).AsByte();
}
