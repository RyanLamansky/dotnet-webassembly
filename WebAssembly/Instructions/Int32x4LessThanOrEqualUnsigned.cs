using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int32x4LessThanOrEqualUnsigned instruction.</summary>
public class Int32x4LessThanOrEqualUnsigned : SimdBinaryV128Instruction, IEquatable<Int32x4LessThanOrEqualUnsigned>
{
    /// <summary>Always <see cref="SimdOpCode.Int32x4LessThanOrEqualUnsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int32x4LessThanOrEqualUnsigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int32x4LeUMethod;

    /// <summary>Creates a new <see cref="Int32x4LessThanOrEqualUnsigned"/> instance.</summary>
    public Int32x4LessThanOrEqualUnsigned() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Int32x4LessThanOrEqualUnsigned;
    /// <inheritdoc/>
    public bool Equals(Int32x4LessThanOrEqualUnsigned? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Int32x4LessThanOrEqualUnsigned;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Int32x4LessThanOrEqualUnsigned;
}
