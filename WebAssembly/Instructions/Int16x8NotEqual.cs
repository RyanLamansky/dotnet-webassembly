using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int16x8NotEqual instruction.</summary>
public class Int16x8NotEqual : SimdBinaryV128Instruction, IEquatable<Int16x8NotEqual>
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8NotEqual"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8NotEqual;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int16x8NotEqualMethod;

    /// <summary>Creates a new <see cref="Int16x8NotEqual"/> instance.</summary>
    public Int16x8NotEqual() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Int16x8NotEqual;
    /// <inheritdoc/>
    public bool Equals(Int16x8NotEqual? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Int16x8NotEqual;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Int16x8NotEqual;
}
