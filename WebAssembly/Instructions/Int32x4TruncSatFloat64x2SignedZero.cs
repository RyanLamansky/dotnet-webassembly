using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int32x4TruncSatFloat64x2SignedZero instruction.</summary>
public class Int32x4TruncSatFloat64x2SignedZero : SimdUnaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int32x4TruncSatFloat64x2SignedZero"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int32x4TruncSatFloat64x2SignedZero;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int32x4TruncSatF64x2SZeroMethod;

    /// <summary>Creates a new <see cref="Int32x4TruncSatFloat64x2SignedZero"/> instance.</summary>
    public Int32x4TruncSatFloat64x2SignedZero() { }
}
