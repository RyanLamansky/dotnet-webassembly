using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int16x8MinSigned instruction.</summary>
public class Int16x8MinSigned : SimdBinaryV128Instruction, IEquatable<Int16x8MinSigned>
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8MinSigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8MinSigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int16x8MinSMethod;

    /// <summary>Creates a new <see cref="Int16x8MinSigned"/> instance.</summary>
    public Int16x8MinSigned() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Int16x8MinSigned;
    /// <inheritdoc/>
    public bool Equals(Int16x8MinSigned? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Int16x8MinSigned;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Int16x8MinSigned;
}
