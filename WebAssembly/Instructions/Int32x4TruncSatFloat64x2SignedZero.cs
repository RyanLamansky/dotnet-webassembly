using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Int32x4TruncSatFloat64x2SignedZero instruction.</summary>
public class Int32x4TruncSatFloat64x2SignedZero : SimdUnaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int32x4TruncSatFloat64x2SignedZero"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int32x4TruncSatFloat64x2SignedZero;

    /// <summary>Creates a new <see cref="Int32x4TruncSatFloat64x2SignedZero"/> instance.</summary>
    public Int32x4TruncSatFloat64x2SignedZero() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a) { var r = new int[4]; for (var i = 0; i < 2; i++) { var f = a.AsDouble().GetElement(i); r[i] = double.IsNaN(f) ? 0 : f >= 2147483647d ? int.MaxValue : f <= -2147483648d ? int.MinValue : (int)f; } return Vector128.Create(r).AsByte(); }
}
