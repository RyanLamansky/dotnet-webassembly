using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Float64x2LessThan instruction.</summary>
public class Float64x2LessThan : SimdBinaryV128Instruction, IEquatable<Float64x2LessThan>
{
    /// <summary>Always <see cref="SimdOpCode.Float64x2LessThan"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float64x2LessThan;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Float64x2LtMethod;

    /// <summary>Creates a new <see cref="Float64x2LessThan"/> instance.</summary>
    public Float64x2LessThan() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Float64x2LessThan;
    /// <inheritdoc/>
    public bool Equals(Float64x2LessThan? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Float64x2LessThan;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Float64x2LessThan;
}
