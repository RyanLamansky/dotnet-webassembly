using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int16x8ExtmulLowInt8x16Unsigned instruction.</summary>
public class Int16x8ExtmulLowInt8x16Unsigned : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8ExtmulLowInt8x16Unsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8ExtmulLowInt8x16Unsigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int16x8ExtmulLowI8x16UMethod;

    /// <summary>Creates a new <see cref="Int16x8ExtmulLowInt8x16Unsigned"/> instance.</summary>
    public Int16x8ExtmulLowInt8x16Unsigned() { }
}
