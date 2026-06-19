using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Float64x2Splat instruction.</summary>
public class Float64x2Splat : SimdSplatInstruction, IEquatable<Float64x2Splat>
{
    /// <summary>Always <see cref="SimdOpCode.Float64x2Splat"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float64x2Splat;
    internal override WebAssemblyValueType ScalarType => WebAssemblyValueType.Float64;
    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Float64x2SplatMethod;

    /// <summary>Creates a new <see cref="Float64x2Splat"/> instance.</summary>
    public Float64x2Splat() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Float64x2Splat;
    /// <inheritdoc/>
    public bool Equals(Float64x2Splat? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Float64x2Splat;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Float64x2Splat;
}
