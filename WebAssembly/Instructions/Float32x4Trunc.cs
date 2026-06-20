using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Float32x4Trunc instruction.</summary>
public class Float32x4Trunc : SimdUnaryV128Instruction, IEquatable<Float32x4Trunc>
{
    /// <summary>Always <see cref="SimdOpCode.Float32x4Trunc"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float32x4Trunc;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Float32x4TruncMethod;

    /// <summary>Creates a new <see cref="Float32x4Trunc"/> instance.</summary>
    public Float32x4Trunc() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Float32x4Trunc;
    /// <inheritdoc/>
    public bool Equals(Float32x4Trunc? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Float32x4Trunc;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Float32x4Trunc;
}
