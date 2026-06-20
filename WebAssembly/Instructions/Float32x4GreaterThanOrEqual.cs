using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Float32x4GreaterThanOrEqual instruction.</summary>
public class Float32x4GreaterThanOrEqual : SimdBinaryV128Instruction, IEquatable<Float32x4GreaterThanOrEqual>
{
    /// <summary>Always <see cref="SimdOpCode.Float32x4GreaterThanOrEqual"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float32x4GreaterThanOrEqual;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Float32x4GeMethod;

    /// <summary>Creates a new <see cref="Float32x4GreaterThanOrEqual"/> instance.</summary>
    public Float32x4GreaterThanOrEqual() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Float32x4GreaterThanOrEqual;
    /// <inheritdoc/>
    public bool Equals(Float32x4GreaterThanOrEqual? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Float32x4GreaterThanOrEqual;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Float32x4GreaterThanOrEqual;
}
