using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int32x4LessThanUnsigned instruction.</summary>
public class Int32x4LessThanUnsigned : SimdBinaryV128Instruction, IEquatable<Int32x4LessThanUnsigned>
{
    /// <summary>Always <see cref="SimdOpCode.Int32x4LessThanUnsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int32x4LessThanUnsigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int32x4LtUMethod;

    /// <summary>Creates a new <see cref="Int32x4LessThanUnsigned"/> instance.</summary>
    public Int32x4LessThanUnsigned() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Int32x4LessThanUnsigned;
    /// <inheritdoc/>
    public bool Equals(Int32x4LessThanUnsigned? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Int32x4LessThanUnsigned;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Int32x4LessThanUnsigned;
}
