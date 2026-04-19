using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int8x16Swizzle instruction.</summary>
public class Int8x16Swizzle : SimdBinaryV128Instruction, IEquatable<Int8x16Swizzle>
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16Swizzle"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16Swizzle;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int8x16SwizzleMethod;

    /// <summary>Creates a new <see cref="Int8x16Swizzle"/> instance.</summary>
    public Int8x16Swizzle() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Int8x16Swizzle;
    /// <inheritdoc/>
    public bool Equals(Int8x16Swizzle? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Int8x16Swizzle;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Int8x16Swizzle;
}
