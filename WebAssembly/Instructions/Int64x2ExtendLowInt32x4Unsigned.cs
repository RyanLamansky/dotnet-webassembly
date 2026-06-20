using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int64x2ExtendLowInt32x4Unsigned instruction.</summary>
public class Int64x2ExtendLowInt32x4Unsigned : SimdUnaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int64x2ExtendLowInt32x4Unsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int64x2ExtendLowInt32x4Unsigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int64x2ExtLowI32x4UMethod;

    /// <summary>Creates a new <see cref="Int64x2ExtendLowInt32x4Unsigned"/> instance.</summary>
    public Int64x2ExtendLowInt32x4Unsigned() { }
}
