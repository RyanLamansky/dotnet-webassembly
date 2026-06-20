using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Float64x2PromoteLowFloat32x4 instruction.</summary>
public class Float64x2PromoteLowFloat32x4 : SimdUnaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Float64x2PromoteLowFloat32x4"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float64x2PromoteLowFloat32x4;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Float64x2PromoteLowF32x4Method;

    /// <summary>Creates a new <see cref="Float64x2PromoteLowFloat32x4"/> instance.</summary>
    public Float64x2PromoteLowFloat32x4() { }
}
