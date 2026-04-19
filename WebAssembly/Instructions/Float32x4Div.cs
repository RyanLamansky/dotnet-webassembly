using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Float32x4Div instruction.</summary>
public class Float32x4Div : SimdBinaryV128Instruction, IEquatable<Float32x4Div>
{
    /// <summary>Always <see cref="SimdOpCode.Float32x4Div"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float32x4Div;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Float32x4DivMethod;

    /// <summary>Creates a new <see cref="Float32x4Div"/> instance.</summary>
    public Float32x4Div() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Float32x4Div;
    /// <inheritdoc/>
    public bool Equals(Float32x4Div? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Float32x4Div;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Float32x4Div;
}
