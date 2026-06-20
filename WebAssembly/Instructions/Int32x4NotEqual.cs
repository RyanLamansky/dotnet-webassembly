using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int32x4NotEqual instruction.</summary>
public class Int32x4NotEqual : SimdBinaryV128Instruction, IEquatable<Int32x4NotEqual>
{
    /// <summary>Always <see cref="SimdOpCode.Int32x4NotEqual"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int32x4NotEqual;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int32x4NotEqualMethod;

    /// <summary>Creates a new <see cref="Int32x4NotEqual"/> instance.</summary>
    public Int32x4NotEqual() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Int32x4NotEqual;
    /// <inheritdoc/>
    public bool Equals(Int32x4NotEqual? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Int32x4NotEqual;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Int32x4NotEqual;
}
