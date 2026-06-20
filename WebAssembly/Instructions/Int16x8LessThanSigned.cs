using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int16x8LessThanSigned instruction.</summary>
public class Int16x8LessThanSigned : SimdBinaryV128Instruction, IEquatable<Int16x8LessThanSigned>
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8LessThanSigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8LessThanSigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int16x8LtSMethod;

    /// <summary>Creates a new <see cref="Int16x8LessThanSigned"/> instance.</summary>
    public Int16x8LessThanSigned() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Int16x8LessThanSigned;
    /// <inheritdoc/>
    public bool Equals(Int16x8LessThanSigned? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Int16x8LessThanSigned;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Int16x8LessThanSigned;
}
