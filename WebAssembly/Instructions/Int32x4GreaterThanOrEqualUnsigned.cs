using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int32x4GreaterThanOrEqualUnsigned instruction.</summary>
public class Int32x4GreaterThanOrEqualUnsigned : SimdBinaryV128Instruction, IEquatable<Int32x4GreaterThanOrEqualUnsigned>
{
    /// <summary>Always <see cref="SimdOpCode.Int32x4GreaterThanOrEqualUnsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int32x4GreaterThanOrEqualUnsigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int32x4GeUMethod;

    /// <summary>Creates a new <see cref="Int32x4GreaterThanOrEqualUnsigned"/> instance.</summary>
    public Int32x4GreaterThanOrEqualUnsigned() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Int32x4GreaterThanOrEqualUnsigned;
    /// <inheritdoc/>
    public bool Equals(Int32x4GreaterThanOrEqualUnsigned? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Int32x4GreaterThanOrEqualUnsigned;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Int32x4GreaterThanOrEqualUnsigned;
}
