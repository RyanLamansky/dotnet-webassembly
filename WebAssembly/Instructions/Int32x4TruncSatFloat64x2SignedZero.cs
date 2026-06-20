using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int32x4TruncSatFloat64x2SignedZero instruction.</summary>
public class Int32x4TruncSatFloat64x2SignedZero : SimdUnaryV128Instruction, IEquatable<Int32x4TruncSatFloat64x2SignedZero>
{
    /// <summary>Always <see cref="SimdOpCode.Int32x4TruncSatFloat64x2SignedZero"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int32x4TruncSatFloat64x2SignedZero;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int32x4TruncSatF64x2SZeroMethod;

    /// <summary>Creates a new <see cref="Int32x4TruncSatFloat64x2SignedZero"/> instance.</summary>
    public Int32x4TruncSatFloat64x2SignedZero() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Int32x4TruncSatFloat64x2SignedZero;
    /// <inheritdoc/>
    public bool Equals(Int32x4TruncSatFloat64x2SignedZero? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Int32x4TruncSatFloat64x2SignedZero;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Int32x4TruncSatFloat64x2SignedZero;
}
