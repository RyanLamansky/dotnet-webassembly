using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int8x16MaxUnsigned instruction.</summary>
public class Int8x16MaxUnsigned : SimdBinaryV128Instruction, IEquatable<Int8x16MaxUnsigned>
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16MaxUnsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16MaxUnsigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int8x16MaxUMethod;

    /// <summary>Creates a new <see cref="Int8x16MaxUnsigned"/> instance.</summary>
    public Int8x16MaxUnsigned() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Int8x16MaxUnsigned;
    /// <inheritdoc/>
    public bool Equals(Int8x16MaxUnsigned? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Int8x16MaxUnsigned;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Int8x16MaxUnsigned;
}
