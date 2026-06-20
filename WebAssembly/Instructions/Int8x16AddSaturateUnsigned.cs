using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int8x16AddSaturateUnsigned instruction.</summary>
public class Int8x16AddSaturateUnsigned : SimdBinaryV128Instruction, IEquatable<Int8x16AddSaturateUnsigned>
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16AddSaturateUnsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16AddSaturateUnsigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int8x16AddSatUMethod;

    /// <summary>Creates a new <see cref="Int8x16AddSaturateUnsigned"/> instance.</summary>
    public Int8x16AddSaturateUnsigned() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Int8x16AddSaturateUnsigned;
    /// <inheritdoc/>
    public bool Equals(Int8x16AddSaturateUnsigned? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Int8x16AddSaturateUnsigned;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Int8x16AddSaturateUnsigned;
}
