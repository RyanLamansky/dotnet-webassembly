using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Float64x2GreaterThanOrEqual instruction.</summary>
public class Float64x2GreaterThanOrEqual : SimdBinaryV128Instruction, IEquatable<Float64x2GreaterThanOrEqual>
{
    /// <summary>Always <see cref="SimdOpCode.Float64x2GreaterThanOrEqual"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float64x2GreaterThanOrEqual;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Float64x2GeMethod;

    /// <summary>Creates a new <see cref="Float64x2GreaterThanOrEqual"/> instance.</summary>
    public Float64x2GreaterThanOrEqual() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Float64x2GreaterThanOrEqual;
    /// <inheritdoc/>
    public bool Equals(Float64x2GreaterThanOrEqual? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Float64x2GreaterThanOrEqual;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Float64x2GreaterThanOrEqual;
}
