using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int8x16MaxSigned instruction.</summary>
public class Int8x16MaxSigned : SimdBinaryV128Instruction, IEquatable<Int8x16MaxSigned>
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16MaxSigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16MaxSigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int8x16MaxSMethod;

    /// <summary>Creates a new <see cref="Int8x16MaxSigned"/> instance.</summary>
    public Int8x16MaxSigned() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Int8x16MaxSigned;
    /// <inheritdoc/>
    public bool Equals(Int8x16MaxSigned? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Int8x16MaxSigned;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Int8x16MaxSigned;
}
