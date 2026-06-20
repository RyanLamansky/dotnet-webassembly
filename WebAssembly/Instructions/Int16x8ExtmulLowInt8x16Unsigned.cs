using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int16x8ExtmulLowInt8x16Unsigned instruction.</summary>
public class Int16x8ExtmulLowInt8x16Unsigned : SimdBinaryV128Instruction, IEquatable<Int16x8ExtmulLowInt8x16Unsigned>
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8ExtmulLowInt8x16Unsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8ExtmulLowInt8x16Unsigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int16x8ExtmulLowI8x16UMethod;

    /// <summary>Creates a new <see cref="Int16x8ExtmulLowInt8x16Unsigned"/> instance.</summary>
    public Int16x8ExtmulLowInt8x16Unsigned() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Int16x8ExtmulLowInt8x16Unsigned;
    /// <inheritdoc/>
    public bool Equals(Int16x8ExtmulLowInt8x16Unsigned? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Int16x8ExtmulLowInt8x16Unsigned;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Int16x8ExtmulLowInt8x16Unsigned;
}
