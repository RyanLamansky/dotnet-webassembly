using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Float32x4Ceil instruction.</summary>
public class Float32x4Ceil : SimdUnaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Float32x4Ceil"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float32x4Ceil;

    /// <summary>Creates a new <see cref="Float32x4Ceil"/> instance.</summary>
    public Float32x4Ceil() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a) => Vector128.Ceiling(a.AsSingle()).AsByte();
}
