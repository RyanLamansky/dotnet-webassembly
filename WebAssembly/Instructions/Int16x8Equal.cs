using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int16x8Equal instruction.</summary>
public class Int16x8Equal : SimdBinaryV128Instruction, IEquatable<Int16x8Equal>
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8Equal"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8Equal;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int16x8EqualMethod;

    /// <summary>Creates a new <see cref="Int16x8Equal"/> instance.</summary>
    public Int16x8Equal() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Int16x8Equal;
    /// <inheritdoc/>
    public bool Equals(Int16x8Equal? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Int16x8Equal;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Int16x8Equal;
}
