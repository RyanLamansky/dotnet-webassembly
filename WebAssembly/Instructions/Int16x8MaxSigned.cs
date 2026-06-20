using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int16x8MaxSigned instruction.</summary>
public class Int16x8MaxSigned : SimdBinaryV128Instruction, IEquatable<Int16x8MaxSigned>
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8MaxSigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8MaxSigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int16x8MaxSMethod;

    /// <summary>Creates a new <see cref="Int16x8MaxSigned"/> instance.</summary>
    public Int16x8MaxSigned() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Int16x8MaxSigned;
    /// <inheritdoc/>
    public bool Equals(Int16x8MaxSigned? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Int16x8MaxSigned;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Int16x8MaxSigned;
}
