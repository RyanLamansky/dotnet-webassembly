using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int16x8AddSaturateUnsigned instruction.</summary>
public class Int16x8AddSaturateUnsigned : SimdBinaryV128Instruction, IEquatable<Int16x8AddSaturateUnsigned>
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8AddSaturateUnsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8AddSaturateUnsigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int16x8AddSatUMethod;

    /// <summary>Creates a new <see cref="Int16x8AddSaturateUnsigned"/> instance.</summary>
    public Int16x8AddSaturateUnsigned() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Int16x8AddSaturateUnsigned;
    /// <inheritdoc/>
    public bool Equals(Int16x8AddSaturateUnsigned? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Int16x8AddSaturateUnsigned;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Int16x8AddSaturateUnsigned;
}
