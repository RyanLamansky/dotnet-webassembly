using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Float32x4DemoteFloat64x2Zero instruction.</summary>
public class Float32x4DemoteFloat64x2Zero : SimdUnaryV128Instruction, IEquatable<Float32x4DemoteFloat64x2Zero>
{
    /// <summary>Always <see cref="SimdOpCode.Float32x4DemoteFloat64x2Zero"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float32x4DemoteFloat64x2Zero;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Float32x4DemoteF64x2ZeroMethod;

    /// <summary>Creates a new <see cref="Float32x4DemoteFloat64x2Zero"/> instance.</summary>
    public Float32x4DemoteFloat64x2Zero() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Float32x4DemoteFloat64x2Zero;
    /// <inheritdoc/>
    public bool Equals(Float32x4DemoteFloat64x2Zero? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Float32x4DemoteFloat64x2Zero;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Float32x4DemoteFloat64x2Zero;
}
