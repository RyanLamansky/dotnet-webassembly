using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Float32x4Floor instruction.</summary>
public class Float32x4Floor : SimdUnaryV128Instruction, IEquatable<Float32x4Floor>
{
    /// <summary>Always <see cref="SimdOpCode.Float32x4Floor"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float32x4Floor;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Float32x4FloorMethod;

    /// <summary>Creates a new <see cref="Float32x4Floor"/> instance.</summary>
    public Float32x4Floor() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Float32x4Floor;
    /// <inheritdoc/>
    public bool Equals(Float32x4Floor? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Float32x4Floor;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Float32x4Floor;
}
