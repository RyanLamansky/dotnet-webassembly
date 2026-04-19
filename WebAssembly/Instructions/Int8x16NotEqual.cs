using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int8x16NotEqual instruction.</summary>
public class Int8x16NotEqual : SimdBinaryV128Instruction, IEquatable<Int8x16NotEqual>
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16NotEqual"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16NotEqual;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int8x16NotEqualMethod;

    /// <summary>Creates a new <see cref="Int8x16NotEqual"/> instance.</summary>
    public Int8x16NotEqual() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Int8x16NotEqual;
    /// <inheritdoc/>
    public bool Equals(Int8x16NotEqual? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Int8x16NotEqual;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Int8x16NotEqual;
}
