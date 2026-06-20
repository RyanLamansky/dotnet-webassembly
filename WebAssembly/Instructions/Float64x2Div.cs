using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Float64x2Div instruction.</summary>
public class Float64x2Div : SimdBinaryV128Instruction, IEquatable<Float64x2Div>
{
    /// <summary>Always <see cref="SimdOpCode.Float64x2Div"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float64x2Div;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Float64x2DivMethod;

    /// <summary>Creates a new <see cref="Float64x2Div"/> instance.</summary>
    public Float64x2Div() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Float64x2Div;
    /// <inheritdoc/>
    public bool Equals(Float64x2Div? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Float64x2Div;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Float64x2Div;
}
