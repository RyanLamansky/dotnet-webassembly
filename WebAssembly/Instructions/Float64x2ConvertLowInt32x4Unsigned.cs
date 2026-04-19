using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Float64x2ConvertLowInt32x4Unsigned instruction.</summary>
public class Float64x2ConvertLowInt32x4Unsigned : SimdUnaryV128Instruction, IEquatable<Float64x2ConvertLowInt32x4Unsigned>
{
    /// <summary>Always <see cref="SimdOpCode.Float64x2ConvertLowInt32x4Unsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float64x2ConvertLowInt32x4Unsigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Float64x2ConvertLowI32x4UMethod;

    /// <summary>Creates a new <see cref="Float64x2ConvertLowInt32x4Unsigned"/> instance.</summary>
    public Float64x2ConvertLowInt32x4Unsigned() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Float64x2ConvertLowInt32x4Unsigned;
    /// <inheritdoc/>
    public bool Equals(Float64x2ConvertLowInt32x4Unsigned? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Float64x2ConvertLowInt32x4Unsigned;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Float64x2ConvertLowInt32x4Unsigned;
}
