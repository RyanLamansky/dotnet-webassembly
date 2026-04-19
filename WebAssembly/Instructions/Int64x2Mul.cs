using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int64x2Mul instruction.</summary>
public class Int64x2Mul : SimdBinaryV128Instruction, IEquatable<Int64x2Mul>
{
    /// <summary>Always <see cref="SimdOpCode.Int64x2Mul"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int64x2Mul;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int64x2MulMethod;

    /// <summary>Creates a new <see cref="Int64x2Mul"/> instance.</summary>
    public Int64x2Mul() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Int64x2Mul;
    /// <inheritdoc/>
    public bool Equals(Int64x2Mul? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Int64x2Mul;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Int64x2Mul;
}
