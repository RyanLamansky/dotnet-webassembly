using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int8x16AddSaturateSigned instruction.</summary>
public class Int8x16AddSaturateSigned : SimdBinaryV128Instruction, IEquatable<Int8x16AddSaturateSigned>
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16AddSaturateSigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16AddSaturateSigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int8x16AddSatSMethod;

    /// <summary>Creates a new <see cref="Int8x16AddSaturateSigned"/> instance.</summary>
    public Int8x16AddSaturateSigned() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Int8x16AddSaturateSigned;
    /// <inheritdoc/>
    public bool Equals(Int8x16AddSaturateSigned? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Int8x16AddSaturateSigned;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Int8x16AddSaturateSigned;
}
