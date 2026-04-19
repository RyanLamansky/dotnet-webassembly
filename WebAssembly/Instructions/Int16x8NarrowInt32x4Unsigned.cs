using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int16x8NarrowInt32x4Unsigned instruction.</summary>
public class Int16x8NarrowInt32x4Unsigned : SimdBinaryV128Instruction, IEquatable<Int16x8NarrowInt32x4Unsigned>
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8NarrowInt32x4Unsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8NarrowInt32x4Unsigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int16x8NarrowI32x4UMethod;

    /// <summary>Creates a new <see cref="Int16x8NarrowInt32x4Unsigned"/> instance.</summary>
    public Int16x8NarrowInt32x4Unsigned() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Int16x8NarrowInt32x4Unsigned;
    /// <inheritdoc/>
    public bool Equals(Int16x8NarrowInt32x4Unsigned? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Int16x8NarrowInt32x4Unsigned;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Int16x8NarrowInt32x4Unsigned;
}
