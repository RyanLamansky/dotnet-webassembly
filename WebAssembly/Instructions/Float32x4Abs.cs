using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Float32x4Abs instruction.</summary>
public class Float32x4Abs : SimdUnaryV128Instruction, IEquatable<Float32x4Abs>
{
    /// <summary>Always <see cref="SimdOpCode.Float32x4Abs"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float32x4Abs;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Float32x4AbsMethod;

    /// <summary>Creates a new <see cref="Float32x4Abs"/> instance.</summary>
    public Float32x4Abs() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Float32x4Abs;
    /// <inheritdoc/>
    public bool Equals(Float32x4Abs? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Float32x4Abs;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Float32x4Abs;
}
