using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Float64x2Ceil instruction.</summary>
public class Float64x2Ceil : SimdUnaryV128Instruction, IEquatable<Float64x2Ceil>
{
    /// <summary>Always <see cref="SimdOpCode.Float64x2Ceil"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float64x2Ceil;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Float64x2CeilMethod;

    /// <summary>Creates a new <see cref="Float64x2Ceil"/> instance.</summary>
    public Float64x2Ceil() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Float64x2Ceil;
    /// <inheritdoc/>
    public bool Equals(Float64x2Ceil? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Float64x2Ceil;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Float64x2Ceil;
}
