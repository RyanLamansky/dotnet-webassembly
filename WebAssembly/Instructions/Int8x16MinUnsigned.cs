using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int8x16MinUnsigned instruction.</summary>
public class Int8x16MinUnsigned : SimdBinaryV128Instruction, IEquatable<Int8x16MinUnsigned>
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16MinUnsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16MinUnsigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int8x16MinUMethod;

    /// <summary>Creates a new <see cref="Int8x16MinUnsigned"/> instance.</summary>
    public Int8x16MinUnsigned() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Int8x16MinUnsigned;
    /// <inheritdoc/>
    public bool Equals(Int8x16MinUnsigned? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Int8x16MinUnsigned;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Int8x16MinUnsigned;
}
