using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Float64x2Sub instruction.</summary>
public class Float64x2Sub : SimdBinaryV128Instruction, IEquatable<Float64x2Sub>
{
    /// <summary>Always <see cref="SimdOpCode.Float64x2Sub"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float64x2Sub;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Float64x2SubMethod;

    /// <summary>Creates a new <see cref="Float64x2Sub"/> instance.</summary>
    public Float64x2Sub() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Float64x2Sub;
    /// <inheritdoc/>
    public bool Equals(Float64x2Sub? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Float64x2Sub;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Float64x2Sub;
}
