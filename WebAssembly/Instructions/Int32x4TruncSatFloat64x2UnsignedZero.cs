using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Truncate f64x2 to i32x4, unsigned, with saturation (zero-extend).</summary>
public class Int32x4TruncSatFloat64x2UnsignedZero : SimdUnaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int32x4TruncSatFloat64x2UnsignedZero"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int32x4TruncSatFloat64x2UnsignedZero;

    /// <summary>Creates a new <see cref="Int32x4TruncSatFloat64x2UnsignedZero"/> instance.</summary>
    public Int32x4TruncSatFloat64x2UnsignedZero() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a) { var r = new uint[4]; for (var i = 0; i < 2; i++) { var f = a.AsDouble().GetElement(i); r[i] = double.IsNaN(f) || f < 0 ? 0u : f >= 4294967295d ? uint.MaxValue : (uint)f; } return Vector128.Create(r).AsByte(); }
}
