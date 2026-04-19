using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int64x2LessThanOrEqualSigned instruction.</summary>
public class Int64x2LessThanOrEqualSigned : SimdBinaryV128Instruction, IEquatable<Int64x2LessThanOrEqualSigned>
{
    /// <summary>Always <see cref="SimdOpCode.Int64x2LessThanOrEqualSigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int64x2LessThanOrEqualSigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int64x2LeSMethod;

    /// <summary>Creates a new <see cref="Int64x2LessThanOrEqualSigned"/> instance.</summary>
    public Int64x2LessThanOrEqualSigned() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Int64x2LessThanOrEqualSigned;
    /// <inheritdoc/>
    public bool Equals(Int64x2LessThanOrEqualSigned? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Int64x2LessThanOrEqualSigned;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Int64x2LessThanOrEqualSigned;
}
