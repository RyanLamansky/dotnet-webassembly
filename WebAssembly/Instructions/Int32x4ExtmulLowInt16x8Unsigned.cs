using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int32x4ExtmulLowInt16x8Unsigned instruction.</summary>
public class Int32x4ExtmulLowInt16x8Unsigned : SimdBinaryV128Instruction, IEquatable<Int32x4ExtmulLowInt16x8Unsigned>
{
    /// <summary>Always <see cref="SimdOpCode.Int32x4ExtmulLowInt16x8Unsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int32x4ExtmulLowInt16x8Unsigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int32x4ExtmulLowI16x8UMethod;

    /// <summary>Creates a new <see cref="Int32x4ExtmulLowInt16x8Unsigned"/> instance.</summary>
    public Int32x4ExtmulLowInt16x8Unsigned() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Int32x4ExtmulLowInt16x8Unsigned;
    /// <inheritdoc/>
    public bool Equals(Int32x4ExtmulLowInt16x8Unsigned? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Int32x4ExtmulLowInt16x8Unsigned;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Int32x4ExtmulLowInt16x8Unsigned;
}
