using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int64x2ExtmulLowInt32x4Signed instruction.</summary>
public class Int64x2ExtmulLowInt32x4Signed : SimdBinaryV128Instruction, IEquatable<Int64x2ExtmulLowInt32x4Signed>
{
    /// <summary>Always <see cref="SimdOpCode.Int64x2ExtmulLowInt32x4Signed"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int64x2ExtmulLowInt32x4Signed;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int64x2ExtmulLowI32x4SMethod;

    /// <summary>Creates a new <see cref="Int64x2ExtmulLowInt32x4Signed"/> instance.</summary>
    public Int64x2ExtmulLowInt32x4Signed() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Int64x2ExtmulLowInt32x4Signed;
    /// <inheritdoc/>
    public bool Equals(Int64x2ExtmulLowInt32x4Signed? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Int64x2ExtmulLowInt32x4Signed;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Int64x2ExtmulLowInt32x4Signed;
}
