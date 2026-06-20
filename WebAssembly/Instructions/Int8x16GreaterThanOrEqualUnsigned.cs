using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int8x16GreaterThanOrEqualUnsigned instruction.</summary>
public class Int8x16GreaterThanOrEqualUnsigned : SimdBinaryV128Instruction, IEquatable<Int8x16GreaterThanOrEqualUnsigned>
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16GreaterThanOrEqualUnsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16GreaterThanOrEqualUnsigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int8x16GeUMethod;

    /// <summary>Creates a new <see cref="Int8x16GreaterThanOrEqualUnsigned"/> instance.</summary>
    public Int8x16GreaterThanOrEqualUnsigned() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Int8x16GreaterThanOrEqualUnsigned;
    /// <inheritdoc/>
    public bool Equals(Int8x16GreaterThanOrEqualUnsigned? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Int8x16GreaterThanOrEqualUnsigned;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Int8x16GreaterThanOrEqualUnsigned;
}
