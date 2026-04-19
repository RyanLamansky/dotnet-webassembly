using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Float64x2Nearest instruction.</summary>
public class Float64x2Nearest : SimdUnaryV128Instruction, IEquatable<Float64x2Nearest>
{
    /// <summary>Always <see cref="SimdOpCode.Float64x2Nearest"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float64x2Nearest;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Float64x2NearestMethod;

    /// <summary>Creates a new <see cref="Float64x2Nearest"/> instance.</summary>
    public Float64x2Nearest() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Float64x2Nearest;
    /// <inheritdoc/>
    public bool Equals(Float64x2Nearest? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Float64x2Nearest;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Float64x2Nearest;
}
