using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Float64x2Pmax instruction.</summary>
public class Float64x2Pmax : SimdBinaryV128Instruction, IEquatable<Float64x2Pmax>
{
    /// <summary>Always <see cref="SimdOpCode.Float64x2Pmax"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float64x2Pmax;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Float64x2PmaxMethod;

    /// <summary>Creates a new <see cref="Float64x2Pmax"/> instance.</summary>
    public Float64x2Pmax() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Float64x2Pmax;
    /// <inheritdoc/>
    public bool Equals(Float64x2Pmax? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Float64x2Pmax;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Float64x2Pmax;
}
