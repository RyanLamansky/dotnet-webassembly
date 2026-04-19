using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int32x4MaxSigned instruction.</summary>
public class Int32x4MaxSigned : SimdBinaryV128Instruction, IEquatable<Int32x4MaxSigned>
{
    /// <summary>Always <see cref="SimdOpCode.Int32x4MaxSigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int32x4MaxSigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int32x4MaxSMethod;

    /// <summary>Creates a new <see cref="Int32x4MaxSigned"/> instance.</summary>
    public Int32x4MaxSigned() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Int32x4MaxSigned;
    /// <inheritdoc/>
    public bool Equals(Int32x4MaxSigned? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Int32x4MaxSigned;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Int32x4MaxSigned;
}
