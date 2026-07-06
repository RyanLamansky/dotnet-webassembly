using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Relaxed truncate f64x2 to i32x4, unsigned (zero-extend); the deterministic profile is identical to <see cref="Int32x4TruncSatFloat64x2UnsignedZero"/>.</summary>
public class Int32x4RelaxedTruncFloat64x2UnsignedZero : SimdUnaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int32x4RelaxedTruncFloat64x2UnsignedZero"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int32x4RelaxedTruncFloat64x2UnsignedZero;

    /// <summary>Creates a new <see cref="Int32x4RelaxedTruncFloat64x2UnsignedZero"/> instance.</summary>
    public Int32x4RelaxedTruncFloat64x2UnsignedZero() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a) => Int32x4TruncSatFloat64x2UnsignedZero.Execute(a);
}
