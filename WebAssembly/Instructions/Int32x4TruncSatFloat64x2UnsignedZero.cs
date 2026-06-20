using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int32x4TruncSatFloat64x2UnsignedZero instruction.</summary>
public class Int32x4TruncSatFloat64x2UnsignedZero : SimdUnaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int32x4TruncSatFloat64x2UnsignedZero"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int32x4TruncSatFloat64x2UnsignedZero;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int32x4TruncSatF64x2UZeroMethod;

    /// <summary>Creates a new <see cref="Int32x4TruncSatFloat64x2UnsignedZero"/> instance.</summary>
    public Int32x4TruncSatFloat64x2UnsignedZero() { }
}
