using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int16x8LessThanOrEqualSigned instruction.</summary>
public class Int16x8LessThanOrEqualSigned : SimdBinaryV128Instruction, IEquatable<Int16x8LessThanOrEqualSigned>
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8LessThanOrEqualSigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8LessThanOrEqualSigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int16x8LeSMethod;

    /// <summary>Creates a new <see cref="Int16x8LessThanOrEqualSigned"/> instance.</summary>
    public Int16x8LessThanOrEqualSigned() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Int16x8LessThanOrEqualSigned;
    /// <inheritdoc/>
    public bool Equals(Int16x8LessThanOrEqualSigned? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Int16x8LessThanOrEqualSigned;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Int16x8LessThanOrEqualSigned;
}
