using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int8x16Neg instruction.</summary>
public class Int8x16Neg : SimdUnaryV128Instruction, IEquatable<Int8x16Neg>
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16Neg"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16Neg;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int8x16NegMethod;

    /// <summary>Creates a new <see cref="Int8x16Neg"/> instance.</summary>
    public Int8x16Neg() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Int8x16Neg;
    /// <inheritdoc/>
    public bool Equals(Int8x16Neg? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Int8x16Neg;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Int8x16Neg;
}
