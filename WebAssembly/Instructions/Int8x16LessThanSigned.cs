using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int8x16LessThanSigned instruction.</summary>
public class Int8x16LessThanSigned : SimdBinaryV128Instruction, IEquatable<Int8x16LessThanSigned>
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16LessThanSigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16LessThanSigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int8x16LtSMethod;

    /// <summary>Creates a new <see cref="Int8x16LessThanSigned"/> instance.</summary>
    public Int8x16LessThanSigned() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Int8x16LessThanSigned;
    /// <inheritdoc/>
    public bool Equals(Int8x16LessThanSigned? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Int8x16LessThanSigned;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Int8x16LessThanSigned;
}
