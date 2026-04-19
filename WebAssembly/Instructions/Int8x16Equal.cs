using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int8x16Equal instruction.</summary>
public class Int8x16Equal : SimdBinaryV128Instruction, IEquatable<Int8x16Equal>
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16Equal"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16Equal;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int8x16EqualMethod;

    /// <summary>Creates a new <see cref="Int8x16Equal"/> instance.</summary>
    public Int8x16Equal() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Int8x16Equal;
    /// <inheritdoc/>
    public bool Equals(Int8x16Equal? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Int8x16Equal;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Int8x16Equal;
}
