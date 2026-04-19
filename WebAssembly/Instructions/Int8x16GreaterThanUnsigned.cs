using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int8x16GreaterThanUnsigned instruction.</summary>
public class Int8x16GreaterThanUnsigned : SimdBinaryV128Instruction, IEquatable<Int8x16GreaterThanUnsigned>
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16GreaterThanUnsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16GreaterThanUnsigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int8x16GtUMethod;

    /// <summary>Creates a new <see cref="Int8x16GreaterThanUnsigned"/> instance.</summary>
    public Int8x16GreaterThanUnsigned() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Int8x16GreaterThanUnsigned;
    /// <inheritdoc/>
    public bool Equals(Int8x16GreaterThanUnsigned? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Int8x16GreaterThanUnsigned;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Int8x16GreaterThanUnsigned;
}
