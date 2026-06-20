using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int32x4ExtmulLowInt16x8Signed instruction.</summary>
public class Int32x4ExtmulLowInt16x8Signed : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int32x4ExtmulLowInt16x8Signed"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int32x4ExtmulLowInt16x8Signed;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int32x4ExtmulLowI16x8SMethod;

    /// <summary>Creates a new <see cref="Int32x4ExtmulLowInt16x8Signed"/> instance.</summary>
    public Int32x4ExtmulLowInt16x8Signed() { }
}
