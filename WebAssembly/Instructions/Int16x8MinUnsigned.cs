using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int16x8MinUnsigned instruction.</summary>
public class Int16x8MinUnsigned : SimdBinaryV128Instruction, IEquatable<Int16x8MinUnsigned>
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8MinUnsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8MinUnsigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int16x8MinUMethod;

    /// <summary>Creates a new <see cref="Int16x8MinUnsigned"/> instance.</summary>
    public Int16x8MinUnsigned() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Int16x8MinUnsigned;
    /// <inheritdoc/>
    public bool Equals(Int16x8MinUnsigned? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Int16x8MinUnsigned;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Int16x8MinUnsigned;
}
