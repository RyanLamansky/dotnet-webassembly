using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int16x8Add instruction.</summary>
public class Int16x8Add : SimdBinaryV128Instruction, IEquatable<Int16x8Add>
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8Add"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8Add;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int16x8AddMethod;

    /// <summary>Creates a new <see cref="Int16x8Add"/> instance.</summary>
    public Int16x8Add() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Int16x8Add;
    /// <inheritdoc/>
    public bool Equals(Int16x8Add? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Int16x8Add;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Int16x8Add;
}
