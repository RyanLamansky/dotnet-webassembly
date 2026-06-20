using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int8x16SubSaturateSigned instruction.</summary>
public class Int8x16SubSaturateSigned : SimdBinaryV128Instruction, IEquatable<Int8x16SubSaturateSigned>
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16SubSaturateSigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16SubSaturateSigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int8x16SubSatSMethod;

    /// <summary>Creates a new <see cref="Int8x16SubSaturateSigned"/> instance.</summary>
    public Int8x16SubSaturateSigned() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Int8x16SubSaturateSigned;
    /// <inheritdoc/>
    public bool Equals(Int8x16SubSaturateSigned? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Int8x16SubSaturateSigned;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Int8x16SubSaturateSigned;
}
