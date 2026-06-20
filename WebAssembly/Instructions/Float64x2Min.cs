using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Float64x2Min instruction.</summary>
public class Float64x2Min : SimdBinaryV128Instruction, IEquatable<Float64x2Min>
{
    /// <summary>Always <see cref="SimdOpCode.Float64x2Min"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float64x2Min;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Float64x2MinMethod;

    /// <summary>Creates a new <see cref="Float64x2Min"/> instance.</summary>
    public Float64x2Min() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Float64x2Min;
    /// <inheritdoc/>
    public bool Equals(Float64x2Min? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Float64x2Min;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Float64x2Min;
}
