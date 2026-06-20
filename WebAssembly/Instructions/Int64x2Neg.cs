using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int64x2Neg instruction.</summary>
public class Int64x2Neg : SimdUnaryV128Instruction, IEquatable<Int64x2Neg>
{
    /// <summary>Always <see cref="SimdOpCode.Int64x2Neg"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int64x2Neg;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int64x2NegMethod;

    /// <summary>Creates a new <see cref="Int64x2Neg"/> instance.</summary>
    public Int64x2Neg() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Int64x2Neg;
    /// <inheritdoc/>
    public bool Equals(Int64x2Neg? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Int64x2Neg;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Int64x2Neg;
}
