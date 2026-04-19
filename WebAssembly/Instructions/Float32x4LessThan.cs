using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Float32x4LessThan instruction.</summary>
public class Float32x4LessThan : SimdBinaryV128Instruction, IEquatable<Float32x4LessThan>
{
    /// <summary>Always <see cref="SimdOpCode.Float32x4LessThan"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float32x4LessThan;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Float32x4LtMethod;

    /// <summary>Creates a new <see cref="Float32x4LessThan"/> instance.</summary>
    public Float32x4LessThan() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Float32x4LessThan;
    /// <inheritdoc/>
    public bool Equals(Float32x4LessThan? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Float32x4LessThan;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Float32x4LessThan;
}
