using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Float64x2Trunc instruction.</summary>
public class Float64x2Trunc : SimdUnaryV128Instruction, IEquatable<Float64x2Trunc>
{
    /// <summary>Always <see cref="SimdOpCode.Float64x2Trunc"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float64x2Trunc;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Float64x2TruncMethod;

    /// <summary>Creates a new <see cref="Float64x2Trunc"/> instance.</summary>
    public Float64x2Trunc() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Float64x2Trunc;
    /// <inheritdoc/>
    public bool Equals(Float64x2Trunc? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Float64x2Trunc;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Float64x2Trunc;
}
