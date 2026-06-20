using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Float64x2Abs instruction.</summary>
public class Float64x2Abs : SimdUnaryV128Instruction, IEquatable<Float64x2Abs>
{
    /// <summary>Always <see cref="SimdOpCode.Float64x2Abs"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float64x2Abs;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Float64x2AbsMethod;

    /// <summary>Creates a new <see cref="Float64x2Abs"/> instance.</summary>
    public Float64x2Abs() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Float64x2Abs;
    /// <inheritdoc/>
    public bool Equals(Float64x2Abs? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Float64x2Abs;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Float64x2Abs;
}
