using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Float64x2ConvertLowInt32x4Signed instruction.</summary>
public class Float64x2ConvertLowInt32x4Signed : SimdUnaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Float64x2ConvertLowInt32x4Signed"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float64x2ConvertLowInt32x4Signed;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Float64x2ConvertLowI32x4SMethod;

    /// <summary>Creates a new <see cref="Float64x2ConvertLowInt32x4Signed"/> instance.</summary>
    public Float64x2ConvertLowInt32x4Signed() { }
}
