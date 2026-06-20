using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Float64x2LessThanOrEqual instruction.</summary>
public class Float64x2LessThanOrEqual : SimdBinaryV128Instruction, IEquatable<Float64x2LessThanOrEqual>
{
    /// <summary>Always <see cref="SimdOpCode.Float64x2LessThanOrEqual"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float64x2LessThanOrEqual;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Float64x2LeMethod;

    /// <summary>Creates a new <see cref="Float64x2LessThanOrEqual"/> instance.</summary>
    public Float64x2LessThanOrEqual() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Float64x2LessThanOrEqual;
    /// <inheritdoc/>
    public bool Equals(Float64x2LessThanOrEqual? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Float64x2LessThanOrEqual;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Float64x2LessThanOrEqual;
}
