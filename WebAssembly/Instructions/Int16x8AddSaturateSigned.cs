using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int16x8AddSaturateSigned instruction.</summary>
public class Int16x8AddSaturateSigned : SimdBinaryV128Instruction, IEquatable<Int16x8AddSaturateSigned>
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8AddSaturateSigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8AddSaturateSigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int16x8AddSatSMethod;

    /// <summary>Creates a new <see cref="Int16x8AddSaturateSigned"/> instance.</summary>
    public Int16x8AddSaturateSigned() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Int16x8AddSaturateSigned;
    /// <inheritdoc/>
    public bool Equals(Int16x8AddSaturateSigned? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Int16x8AddSaturateSigned;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Int16x8AddSaturateSigned;
}
