using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int8x16MinSigned instruction.</summary>
public class Int8x16MinSigned : SimdBinaryV128Instruction, IEquatable<Int8x16MinSigned>
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16MinSigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16MinSigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int8x16MinSMethod;

    /// <summary>Creates a new <see cref="Int8x16MinSigned"/> instance.</summary>
    public Int8x16MinSigned() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Int8x16MinSigned;
    /// <inheritdoc/>
    public bool Equals(Int8x16MinSigned? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Int8x16MinSigned;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Int8x16MinSigned;
}
