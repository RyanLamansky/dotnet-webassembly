using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Float32x4ConvertInt32x4Unsigned instruction.</summary>
public class Float32x4ConvertInt32x4Unsigned : SimdUnaryV128Instruction, IEquatable<Float32x4ConvertInt32x4Unsigned>
{
    /// <summary>Always <see cref="SimdOpCode.Float32x4ConvertInt32x4Unsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float32x4ConvertInt32x4Unsigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Float32x4ConvertI32x4UMethod;

    /// <summary>Creates a new <see cref="Float32x4ConvertInt32x4Unsigned"/> instance.</summary>
    public Float32x4ConvertInt32x4Unsigned() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Float32x4ConvertInt32x4Unsigned;
    /// <inheritdoc/>
    public bool Equals(Float32x4ConvertInt32x4Unsigned? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Float32x4ConvertInt32x4Unsigned;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Float32x4ConvertInt32x4Unsigned;
}
