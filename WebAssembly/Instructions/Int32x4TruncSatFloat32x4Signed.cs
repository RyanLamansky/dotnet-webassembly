using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Truncate f32x4 to i32x4, signed, with saturation.</summary>
public class Int32x4TruncSatFloat32x4Signed : SimdUnaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int32x4TruncSatFloat32x4Signed"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int32x4TruncSatFloat32x4Signed;

    /// <summary>Creates a new <see cref="Int32x4TruncSatFloat32x4Signed"/> instance.</summary>
    public Int32x4TruncSatFloat32x4Signed() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a) { var r = new int[4]; for (var i = 0; i < 4; i++) { var f = a.AsSingle().GetElement(i); r[i] = float.IsNaN(f) ? 0 : f >= 2147483647f ? int.MaxValue : f <= -2147483648f ? int.MinValue : (int)f; } return Vector128.Create(r).AsByte(); }
}
