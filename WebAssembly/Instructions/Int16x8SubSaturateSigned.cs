using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int16x8SubSaturateSigned instruction.</summary>
public class Int16x8SubSaturateSigned : SimdBinaryV128Instruction, IEquatable<Int16x8SubSaturateSigned>
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8SubSaturateSigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8SubSaturateSigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int16x8SubSatSMethod;

    /// <summary>Creates a new <see cref="Int16x8SubSaturateSigned"/> instance.</summary>
    public Int16x8SubSaturateSigned() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Int16x8SubSaturateSigned;
    /// <inheritdoc/>
    public bool Equals(Int16x8SubSaturateSigned? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Int16x8SubSaturateSigned;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Int16x8SubSaturateSigned;
}
