using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int32x4Add instruction.</summary>
public class Int32x4Add : SimdBinaryV128Instruction, IEquatable<Int32x4Add>
{
    /// <summary>Always <see cref="SimdOpCode.Int32x4Add"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int32x4Add;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int32x4AddMethod;

    /// <summary>Creates a new <see cref="Int32x4Add"/> instance.</summary>
    public Int32x4Add() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Int32x4Add;
    /// <inheritdoc/>
    public bool Equals(Int32x4Add? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Int32x4Add;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Int32x4Add;
}
