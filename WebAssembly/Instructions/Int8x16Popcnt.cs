using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int8x16Popcnt instruction.</summary>
public class Int8x16Popcnt : SimdUnaryV128Instruction, IEquatable<Int8x16Popcnt>
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16Popcnt"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16Popcnt;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int8x16PopcntMethod;

    /// <summary>Creates a new <see cref="Int8x16Popcnt"/> instance.</summary>
    public Int8x16Popcnt() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Int8x16Popcnt;
    /// <inheritdoc/>
    public bool Equals(Int8x16Popcnt? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Int8x16Popcnt;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Int8x16Popcnt;
}
