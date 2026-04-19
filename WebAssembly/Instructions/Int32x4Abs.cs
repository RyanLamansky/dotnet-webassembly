using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int32x4Abs instruction.</summary>
public class Int32x4Abs : SimdUnaryV128Instruction, IEquatable<Int32x4Abs>
{
    /// <summary>Always <see cref="SimdOpCode.Int32x4Abs"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int32x4Abs;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int32x4AbsMethod;

    /// <summary>Creates a new <see cref="Int32x4Abs"/> instance.</summary>
    public Int32x4Abs() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Int32x4Abs;
    /// <inheritdoc/>
    public bool Equals(Int32x4Abs? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Int32x4Abs;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Int32x4Abs;
}
