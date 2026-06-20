using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Float64x2GreaterThan instruction.</summary>
public class Float64x2GreaterThan : SimdBinaryV128Instruction, IEquatable<Float64x2GreaterThan>
{
    /// <summary>Always <see cref="SimdOpCode.Float64x2GreaterThan"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float64x2GreaterThan;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Float64x2GtMethod;

    /// <summary>Creates a new <see cref="Float64x2GreaterThan"/> instance.</summary>
    public Float64x2GreaterThan() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Float64x2GreaterThan;
    /// <inheritdoc/>
    public bool Equals(Float64x2GreaterThan? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Float64x2GreaterThan;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Float64x2GreaterThan;
}
