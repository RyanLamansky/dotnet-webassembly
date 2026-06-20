using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int16x8Neg instruction.</summary>
public class Int16x8Neg : SimdUnaryV128Instruction, IEquatable<Int16x8Neg>
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8Neg"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8Neg;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int16x8NegMethod;

    /// <summary>Creates a new <see cref="Int16x8Neg"/> instance.</summary>
    public Int16x8Neg() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Int16x8Neg;
    /// <inheritdoc/>
    public bool Equals(Int16x8Neg? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Int16x8Neg;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Int16x8Neg;
}
