using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int8x16Abs instruction.</summary>
public class Int8x16Abs : SimdUnaryV128Instruction, IEquatable<Int8x16Abs>
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16Abs"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16Abs;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int8x16AbsMethod;

    /// <summary>Creates a new <see cref="Int8x16Abs"/> instance.</summary>
    public Int8x16Abs() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Int8x16Abs;
    /// <inheritdoc/>
    public bool Equals(Int8x16Abs? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Int8x16Abs;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Int8x16Abs;
}
