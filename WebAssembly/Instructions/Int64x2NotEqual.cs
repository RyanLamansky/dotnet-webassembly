using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int64x2NotEqual instruction.</summary>
public class Int64x2NotEqual : SimdBinaryV128Instruction, IEquatable<Int64x2NotEqual>
{
    /// <summary>Always <see cref="SimdOpCode.Int64x2NotEqual"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int64x2NotEqual;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int64x2NotEqualMethod;

    /// <summary>Creates a new <see cref="Int64x2NotEqual"/> instance.</summary>
    public Int64x2NotEqual() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Int64x2NotEqual;
    /// <inheritdoc/>
    public bool Equals(Int64x2NotEqual? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Int64x2NotEqual;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Int64x2NotEqual;
}
