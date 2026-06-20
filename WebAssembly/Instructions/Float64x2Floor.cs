using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Float64x2Floor instruction.</summary>
public class Float64x2Floor : SimdUnaryV128Instruction, IEquatable<Float64x2Floor>
{
    /// <summary>Always <see cref="SimdOpCode.Float64x2Floor"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float64x2Floor;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Float64x2FloorMethod;

    /// <summary>Creates a new <see cref="Float64x2Floor"/> instance.</summary>
    public Float64x2Floor() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Float64x2Floor;
    /// <inheritdoc/>
    public bool Equals(Float64x2Floor? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Float64x2Floor;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Float64x2Floor;
}
