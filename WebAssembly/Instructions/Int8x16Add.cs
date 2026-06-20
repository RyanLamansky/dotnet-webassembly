using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int8x16Add instruction.</summary>
public class Int8x16Add : SimdBinaryV128Instruction, IEquatable<Int8x16Add>
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16Add"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16Add;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int8x16AddMethod;

    /// <summary>Creates a new <see cref="Int8x16Add"/> instance.</summary>
    public Int8x16Add() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Int8x16Add;
    /// <inheritdoc/>
    public bool Equals(Int8x16Add? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Int8x16Add;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Int8x16Add;
}
