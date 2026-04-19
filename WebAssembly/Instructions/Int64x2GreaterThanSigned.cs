using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int64x2GreaterThanSigned instruction.</summary>
public class Int64x2GreaterThanSigned : SimdBinaryV128Instruction, IEquatable<Int64x2GreaterThanSigned>
{
    /// <summary>Always <see cref="SimdOpCode.Int64x2GreaterThanSigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int64x2GreaterThanSigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int64x2GtSMethod;

    /// <summary>Creates a new <see cref="Int64x2GreaterThanSigned"/> instance.</summary>
    public Int64x2GreaterThanSigned() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Int64x2GreaterThanSigned;
    /// <inheritdoc/>
    public bool Equals(Int64x2GreaterThanSigned? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Int64x2GreaterThanSigned;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Int64x2GreaterThanSigned;
}
