using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int64x2GreaterThanOrEqualSigned instruction.</summary>
public class Int64x2GreaterThanOrEqualSigned : SimdBinaryV128Instruction, IEquatable<Int64x2GreaterThanOrEqualSigned>
{
    /// <summary>Always <see cref="SimdOpCode.Int64x2GreaterThanOrEqualSigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int64x2GreaterThanOrEqualSigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int64x2GeSMethod;

    /// <summary>Creates a new <see cref="Int64x2GreaterThanOrEqualSigned"/> instance.</summary>
    public Int64x2GreaterThanOrEqualSigned() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Int64x2GreaterThanOrEqualSigned;
    /// <inheritdoc/>
    public bool Equals(Int64x2GreaterThanOrEqualSigned? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Int64x2GreaterThanOrEqualSigned;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Int64x2GreaterThanOrEqualSigned;
}
