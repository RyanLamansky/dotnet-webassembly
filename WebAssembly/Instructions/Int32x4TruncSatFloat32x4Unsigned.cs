using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Int32x4TruncSatFloat32x4Unsigned instruction.</summary>
public class Int32x4TruncSatFloat32x4Unsigned : SimdUnaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int32x4TruncSatFloat32x4Unsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int32x4TruncSatFloat32x4Unsigned;

    /// <summary>Creates a new <see cref="Int32x4TruncSatFloat32x4Unsigned"/> instance.</summary>
    public Int32x4TruncSatFloat32x4Unsigned() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a) { var r = new uint[4]; for (var i = 0; i < 4; i++) { var f = a.AsSingle().GetElement(i); r[i] = float.IsNaN(f) || f < 0 ? 0u : f >= 4294967295f ? uint.MaxValue : (uint)f; } return Vector128.Create(r).AsByte(); }
}
