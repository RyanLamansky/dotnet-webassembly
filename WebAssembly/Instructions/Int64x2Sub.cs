using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int64x2Sub instruction.</summary>
public class Int64x2Sub : SimdBinaryV128Instruction, IEquatable<Int64x2Sub>
{
    /// <summary>Always <see cref="SimdOpCode.Int64x2Sub"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int64x2Sub;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int64x2SubMethod;

    /// <summary>Creates a new <see cref="Int64x2Sub"/> instance.</summary>
    public Int64x2Sub() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Int64x2Sub;
    /// <inheritdoc/>
    public bool Equals(Int64x2Sub? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Int64x2Sub;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Int64x2Sub;
}
