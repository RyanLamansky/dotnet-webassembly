using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int32x4ExtaddPairwiseInt16x8Unsigned instruction.</summary>
public class Int32x4ExtaddPairwiseInt16x8Unsigned : SimdUnaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int32x4ExtaddPairwiseInt16x8Unsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int32x4ExtaddPairwiseInt16x8Unsigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int32x4ExtaddPairwiseI16x8UMethod;

    /// <summary>Creates a new <see cref="Int32x4ExtaddPairwiseInt16x8Unsigned"/> instance.</summary>
    public Int32x4ExtaddPairwiseInt16x8Unsigned() { }
}
