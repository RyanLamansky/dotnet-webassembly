using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int32x4MinSigned instruction.</summary>
public class Int32x4MinSigned : SimdBinaryV128Instruction, IEquatable<Int32x4MinSigned>
{
    /// <summary>Always <see cref="SimdOpCode.Int32x4MinSigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int32x4MinSigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int32x4MinSMethod;

    /// <summary>Creates a new <see cref="Int32x4MinSigned"/> instance.</summary>
    public Int32x4MinSigned() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Int32x4MinSigned;
    /// <inheritdoc/>
    public bool Equals(Int32x4MinSigned? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Int32x4MinSigned;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Int32x4MinSigned;
}
