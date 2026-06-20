using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Float32x4Add instruction.</summary>
public class Float32x4Add : SimdBinaryV128Instruction, IEquatable<Float32x4Add>
{
    /// <summary>Always <see cref="SimdOpCode.Float32x4Add"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float32x4Add;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Float32x4AddMethod;

    /// <summary>Creates a new <see cref="Float32x4Add"/> instance.</summary>
    public Float32x4Add() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Float32x4Add;
    /// <inheritdoc/>
    public bool Equals(Float32x4Add? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Float32x4Add;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Float32x4Add;
}
