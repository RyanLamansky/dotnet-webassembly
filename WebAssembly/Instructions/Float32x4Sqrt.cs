using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Float32x4Sqrt instruction.</summary>
public class Float32x4Sqrt : SimdUnaryV128Instruction, IEquatable<Float32x4Sqrt>
{
    /// <summary>Always <see cref="SimdOpCode.Float32x4Sqrt"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float32x4Sqrt;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Float32x4SqrtMethod;

    /// <summary>Creates a new <see cref="Float32x4Sqrt"/> instance.</summary>
    public Float32x4Sqrt() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Float32x4Sqrt;
    /// <inheritdoc/>
    public bool Equals(Float32x4Sqrt? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Float32x4Sqrt;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Float32x4Sqrt;
}
