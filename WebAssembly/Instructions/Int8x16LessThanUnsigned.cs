using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int8x16LessThanUnsigned instruction.</summary>
public class Int8x16LessThanUnsigned : SimdBinaryV128Instruction, IEquatable<Int8x16LessThanUnsigned>
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16LessThanUnsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16LessThanUnsigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int8x16LtUMethod;

    /// <summary>Creates a new <see cref="Int8x16LessThanUnsigned"/> instance.</summary>
    public Int8x16LessThanUnsigned() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Int8x16LessThanUnsigned;
    /// <inheritdoc/>
    public bool Equals(Int8x16LessThanUnsigned? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Int8x16LessThanUnsigned;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Int8x16LessThanUnsigned;
}
