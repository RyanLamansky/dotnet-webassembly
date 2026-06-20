using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Float32x4Equal instruction.</summary>
public class Float32x4Equal : SimdBinaryV128Instruction, IEquatable<Float32x4Equal>
{
    /// <summary>Always <see cref="SimdOpCode.Float32x4Equal"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float32x4Equal;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Float32x4EqualMethod;

    /// <summary>Creates a new <see cref="Float32x4Equal"/> instance.</summary>
    public Float32x4Equal() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Float32x4Equal;
    /// <inheritdoc/>
    public bool Equals(Float32x4Equal? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Float32x4Equal;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Float32x4Equal;
}
