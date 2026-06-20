using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int16x8SubSaturateUnsigned instruction.</summary>
public class Int16x8SubSaturateUnsigned : SimdBinaryV128Instruction, IEquatable<Int16x8SubSaturateUnsigned>
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8SubSaturateUnsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8SubSaturateUnsigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int16x8SubSatUMethod;

    /// <summary>Creates a new <see cref="Int16x8SubSaturateUnsigned"/> instance.</summary>
    public Int16x8SubSaturateUnsigned() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Int16x8SubSaturateUnsigned;
    /// <inheritdoc/>
    public bool Equals(Int16x8SubSaturateUnsigned? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Int16x8SubSaturateUnsigned;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Int16x8SubSaturateUnsigned;
}
