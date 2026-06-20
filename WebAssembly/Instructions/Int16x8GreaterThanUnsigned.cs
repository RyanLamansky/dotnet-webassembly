using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int16x8GreaterThanUnsigned instruction.</summary>
public class Int16x8GreaterThanUnsigned : SimdBinaryV128Instruction, IEquatable<Int16x8GreaterThanUnsigned>
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8GreaterThanUnsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8GreaterThanUnsigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int16x8GtUMethod;

    /// <summary>Creates a new <see cref="Int16x8GreaterThanUnsigned"/> instance.</summary>
    public Int16x8GreaterThanUnsigned() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Int16x8GreaterThanUnsigned;
    /// <inheritdoc/>
    public bool Equals(Int16x8GreaterThanUnsigned? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Int16x8GreaterThanUnsigned;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Int16x8GreaterThanUnsigned;
}
