using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int16x8ExtaddPairwiseInt8x16Signed instruction.</summary>
public class Int16x8ExtaddPairwiseInt8x16Signed : SimdUnaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8ExtaddPairwiseInt8x16Signed"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8ExtaddPairwiseInt8x16Signed;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int16x8ExtaddPairwiseI8x16SMethod;

    /// <summary>Creates a new <see cref="Int16x8ExtaddPairwiseInt8x16Signed"/> instance.</summary>
    public Int16x8ExtaddPairwiseInt8x16Signed() { }
}
