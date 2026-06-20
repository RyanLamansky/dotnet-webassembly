using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Float64x2Neg instruction.</summary>
public class Float64x2Neg : SimdUnaryV128Instruction, IEquatable<Float64x2Neg>
{
    /// <summary>Always <see cref="SimdOpCode.Float64x2Neg"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float64x2Neg;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Float64x2NegMethod;

    /// <summary>Creates a new <see cref="Float64x2Neg"/> instance.</summary>
    public Float64x2Neg() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Float64x2Neg;
    /// <inheritdoc/>
    public bool Equals(Float64x2Neg? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Float64x2Neg;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Float64x2Neg;
}
