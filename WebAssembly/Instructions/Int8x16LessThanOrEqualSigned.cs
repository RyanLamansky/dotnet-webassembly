using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int8x16LessThanOrEqualSigned instruction.</summary>
public class Int8x16LessThanOrEqualSigned : SimdBinaryV128Instruction, IEquatable<Int8x16LessThanOrEqualSigned>
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16LessThanOrEqualSigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16LessThanOrEqualSigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int8x16LeSMethod;

    /// <summary>Creates a new <see cref="Int8x16LessThanOrEqualSigned"/> instance.</summary>
    public Int8x16LessThanOrEqualSigned() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Int8x16LessThanOrEqualSigned;
    /// <inheritdoc/>
    public bool Equals(Int8x16LessThanOrEqualSigned? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Int8x16LessThanOrEqualSigned;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Int8x16LessThanOrEqualSigned;
}
