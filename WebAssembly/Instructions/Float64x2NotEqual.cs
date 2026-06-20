using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Float64x2NotEqual instruction.</summary>
public class Float64x2NotEqual : SimdBinaryV128Instruction, IEquatable<Float64x2NotEqual>
{
    /// <summary>Always <see cref="SimdOpCode.Float64x2NotEqual"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float64x2NotEqual;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Float64x2NotEqualMethod;

    /// <summary>Creates a new <see cref="Float64x2NotEqual"/> instance.</summary>
    public Float64x2NotEqual() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Float64x2NotEqual;
    /// <inheritdoc/>
    public bool Equals(Float64x2NotEqual? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Float64x2NotEqual;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Float64x2NotEqual;
}
