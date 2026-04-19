using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int8x16GreaterThanSigned instruction.</summary>
public class Int8x16GreaterThanSigned : SimdBinaryV128Instruction, IEquatable<Int8x16GreaterThanSigned>
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16GreaterThanSigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16GreaterThanSigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int8x16GtSMethod;

    /// <summary>Creates a new <see cref="Int8x16GreaterThanSigned"/> instance.</summary>
    public Int8x16GreaterThanSigned() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Int8x16GreaterThanSigned;
    /// <inheritdoc/>
    public bool Equals(Int8x16GreaterThanSigned? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Int8x16GreaterThanSigned;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Int8x16GreaterThanSigned;
}
