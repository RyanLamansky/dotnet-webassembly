using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int16x8LessThanOrEqualUnsigned instruction.</summary>
public class Int16x8LessThanOrEqualUnsigned : SimdBinaryV128Instruction, IEquatable<Int16x8LessThanOrEqualUnsigned>
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8LessThanOrEqualUnsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8LessThanOrEqualUnsigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int16x8LeUMethod;

    /// <summary>Creates a new <see cref="Int16x8LessThanOrEqualUnsigned"/> instance.</summary>
    public Int16x8LessThanOrEqualUnsigned() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Int16x8LessThanOrEqualUnsigned;
    /// <inheritdoc/>
    public bool Equals(Int16x8LessThanOrEqualUnsigned? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Int16x8LessThanOrEqualUnsigned;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Int16x8LessThanOrEqualUnsigned;
}
