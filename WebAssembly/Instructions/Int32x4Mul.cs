using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int32x4Mul instruction.</summary>
public class Int32x4Mul : SimdBinaryV128Instruction, IEquatable<Int32x4Mul>
{
    /// <summary>Always <see cref="SimdOpCode.Int32x4Mul"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int32x4Mul;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int32x4MulMethod;

    /// <summary>Creates a new <see cref="Int32x4Mul"/> instance.</summary>
    public Int32x4Mul() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Int32x4Mul;
    /// <inheritdoc/>
    public bool Equals(Int32x4Mul? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Int32x4Mul;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Int32x4Mul;
}
