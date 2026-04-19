using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int16x8MaxUnsigned instruction.</summary>
public class Int16x8MaxUnsigned : SimdBinaryV128Instruction, IEquatable<Int16x8MaxUnsigned>
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8MaxUnsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8MaxUnsigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int16x8MaxUMethod;

    /// <summary>Creates a new <see cref="Int16x8MaxUnsigned"/> instance.</summary>
    public Int16x8MaxUnsigned() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Int16x8MaxUnsigned;
    /// <inheritdoc/>
    public bool Equals(Int16x8MaxUnsigned? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Int16x8MaxUnsigned;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Int16x8MaxUnsigned;
}
