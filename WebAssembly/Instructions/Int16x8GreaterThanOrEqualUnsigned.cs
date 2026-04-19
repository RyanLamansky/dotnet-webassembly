using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int16x8GreaterThanOrEqualUnsigned instruction.</summary>
public class Int16x8GreaterThanOrEqualUnsigned : SimdBinaryV128Instruction, IEquatable<Int16x8GreaterThanOrEqualUnsigned>
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8GreaterThanOrEqualUnsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8GreaterThanOrEqualUnsigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int16x8GeUMethod;

    /// <summary>Creates a new <see cref="Int16x8GreaterThanOrEqualUnsigned"/> instance.</summary>
    public Int16x8GreaterThanOrEqualUnsigned() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Int16x8GreaterThanOrEqualUnsigned;
    /// <inheritdoc/>
    public bool Equals(Int16x8GreaterThanOrEqualUnsigned? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Int16x8GreaterThanOrEqualUnsigned;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Int16x8GreaterThanOrEqualUnsigned;
}
