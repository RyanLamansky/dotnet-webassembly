using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int64x2Bitmask instruction.</summary>
public class Int64x2Bitmask : SimdV128ToI32Instruction, IEquatable<Int64x2Bitmask>
{
    /// <summary>Always <see cref="SimdOpCode.Int64x2Bitmask"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int64x2Bitmask;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int64x2BitmaskMethod;

    /// <summary>Creates a new <see cref="Int64x2Bitmask"/> instance.</summary>
    public Int64x2Bitmask() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Int64x2Bitmask;
    /// <inheritdoc/>
    public bool Equals(Int64x2Bitmask? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Int64x2Bitmask;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Int64x2Bitmask;
}
