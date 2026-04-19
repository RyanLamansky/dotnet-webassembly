using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Float32x4Mul instruction.</summary>
public class Float32x4Mul : SimdBinaryV128Instruction, IEquatable<Float32x4Mul>
{
    /// <summary>Always <see cref="SimdOpCode.Float32x4Mul"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float32x4Mul;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Float32x4MulMethod;

    /// <summary>Creates a new <see cref="Float32x4Mul"/> instance.</summary>
    public Float32x4Mul() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Float32x4Mul;
    /// <inheritdoc/>
    public bool Equals(Float32x4Mul? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Float32x4Mul;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Float32x4Mul;
}
