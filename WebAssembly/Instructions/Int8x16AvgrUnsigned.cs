using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int8x16AvgrUnsigned instruction.</summary>
public class Int8x16AvgrUnsigned : SimdBinaryV128Instruction, IEquatable<Int8x16AvgrUnsigned>
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16AvgrUnsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16AvgrUnsigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int8x16AvgrUMethod;

    /// <summary>Creates a new <see cref="Int8x16AvgrUnsigned"/> instance.</summary>
    public Int8x16AvgrUnsigned() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Int8x16AvgrUnsigned;
    /// <inheritdoc/>
    public bool Equals(Int8x16AvgrUnsigned? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Int8x16AvgrUnsigned;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Int8x16AvgrUnsigned;
}
