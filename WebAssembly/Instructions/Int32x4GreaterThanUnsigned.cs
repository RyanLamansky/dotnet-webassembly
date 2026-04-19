using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int32x4GreaterThanUnsigned instruction.</summary>
public class Int32x4GreaterThanUnsigned : SimdBinaryV128Instruction, IEquatable<Int32x4GreaterThanUnsigned>
{
    /// <summary>Always <see cref="SimdOpCode.Int32x4GreaterThanUnsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int32x4GreaterThanUnsigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int32x4GtUMethod;

    /// <summary>Creates a new <see cref="Int32x4GreaterThanUnsigned"/> instance.</summary>
    public Int32x4GreaterThanUnsigned() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Int32x4GreaterThanUnsigned;
    /// <inheritdoc/>
    public bool Equals(Int32x4GreaterThanUnsigned? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Int32x4GreaterThanUnsigned;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Int32x4GreaterThanUnsigned;
}
