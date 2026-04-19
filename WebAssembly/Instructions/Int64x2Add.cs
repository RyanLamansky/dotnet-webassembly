using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int64x2Add instruction.</summary>
public class Int64x2Add : SimdBinaryV128Instruction, IEquatable<Int64x2Add>
{
    /// <summary>Always <see cref="SimdOpCode.Int64x2Add"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int64x2Add;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int64x2AddMethod;

    /// <summary>Creates a new <see cref="Int64x2Add"/> instance.</summary>
    public Int64x2Add() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Int64x2Add;
    /// <inheritdoc/>
    public bool Equals(Int64x2Add? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Int64x2Add;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Int64x2Add;
}
