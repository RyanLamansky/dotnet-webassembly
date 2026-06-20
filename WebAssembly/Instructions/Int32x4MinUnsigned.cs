using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int32x4MinUnsigned instruction.</summary>
public class Int32x4MinUnsigned : SimdBinaryV128Instruction, IEquatable<Int32x4MinUnsigned>
{
    /// <summary>Always <see cref="SimdOpCode.Int32x4MinUnsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int32x4MinUnsigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int32x4MinUMethod;

    /// <summary>Creates a new <see cref="Int32x4MinUnsigned"/> instance.</summary>
    public Int32x4MinUnsigned() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Int32x4MinUnsigned;
    /// <inheritdoc/>
    public bool Equals(Int32x4MinUnsigned? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Int32x4MinUnsigned;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Int32x4MinUnsigned;
}
