using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Float32x4Nearest instruction.</summary>
public class Float32x4Nearest : SimdUnaryV128Instruction, IEquatable<Float32x4Nearest>
{
    /// <summary>Always <see cref="SimdOpCode.Float32x4Nearest"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float32x4Nearest;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Float32x4NearestMethod;

    /// <summary>Creates a new <see cref="Float32x4Nearest"/> instance.</summary>
    public Float32x4Nearest() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Float32x4Nearest;
    /// <inheritdoc/>
    public bool Equals(Float32x4Nearest? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Float32x4Nearest;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Float32x4Nearest;
}
