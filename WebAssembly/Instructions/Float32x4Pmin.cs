using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Float32x4Pmin instruction.</summary>
public class Float32x4Pmin : SimdBinaryV128Instruction, IEquatable<Float32x4Pmin>
{
    /// <summary>Always <see cref="SimdOpCode.Float32x4Pmin"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float32x4Pmin;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Float32x4PminMethod;

    /// <summary>Creates a new <see cref="Float32x4Pmin"/> instance.</summary>
    public Float32x4Pmin() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Float32x4Pmin;
    /// <inheritdoc/>
    public bool Equals(Float32x4Pmin? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Float32x4Pmin;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Float32x4Pmin;
}
