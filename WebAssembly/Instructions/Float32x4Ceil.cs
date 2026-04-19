using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Float32x4Ceil instruction.</summary>
public class Float32x4Ceil : SimdUnaryV128Instruction, IEquatable<Float32x4Ceil>
{
    /// <summary>Always <see cref="SimdOpCode.Float32x4Ceil"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float32x4Ceil;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Float32x4CeilMethod;

    /// <summary>Creates a new <see cref="Float32x4Ceil"/> instance.</summary>
    public Float32x4Ceil() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Float32x4Ceil;
    /// <inheritdoc/>
    public bool Equals(Float32x4Ceil? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Float32x4Ceil;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Float32x4Ceil;
}
