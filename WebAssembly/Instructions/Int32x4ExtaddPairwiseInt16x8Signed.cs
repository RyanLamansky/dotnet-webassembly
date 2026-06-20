using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int32x4ExtaddPairwiseInt16x8Signed instruction.</summary>
public class Int32x4ExtaddPairwiseInt16x8Signed : SimdUnaryV128Instruction, IEquatable<Int32x4ExtaddPairwiseInt16x8Signed>
{
    /// <summary>Always <see cref="SimdOpCode.Int32x4ExtaddPairwiseInt16x8Signed"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int32x4ExtaddPairwiseInt16x8Signed;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int32x4ExtaddPairwiseI16x8SMethod;

    /// <summary>Creates a new <see cref="Int32x4ExtaddPairwiseInt16x8Signed"/> instance.</summary>
    public Int32x4ExtaddPairwiseInt16x8Signed() { }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Int32x4ExtaddPairwiseInt16x8Signed;
    /// <inheritdoc/>
    public bool Equals(Int32x4ExtaddPairwiseInt16x8Signed? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is Int32x4ExtaddPairwiseInt16x8Signed;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.Int32x4ExtaddPairwiseInt16x8Signed;
}
