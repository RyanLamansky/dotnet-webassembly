using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Float64x2Sqrt instruction.</summary>
public class Float64x2Sqrt : SimdUnaryV128Instruction, IEquatable<Float64x2Sqrt>
{
    /// <summary>Always <see cref="SimdOpCode.Float64x2Sqrt"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float64x2Sqrt;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Float64x2SqrtMethod;

    /// <summary>Creates a new <see cref="Float64x2Sqrt"/> instance.</summary>
    public Float64x2Sqrt() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Float64x2Sqrt;
    /// <inheritdoc/>
    public bool Equals(Float64x2Sqrt? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Float64x2Sqrt;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Float64x2Sqrt;
}
