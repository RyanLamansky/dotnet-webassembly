using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Float32x4DemoteFloat64x2Zero instruction.</summary>
public class Float32x4DemoteFloat64x2Zero : SimdUnaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Float32x4DemoteFloat64x2Zero"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float32x4DemoteFloat64x2Zero;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Float32x4DemoteF64x2ZeroMethod;

    /// <summary>Creates a new <see cref="Float32x4DemoteFloat64x2Zero"/> instance.</summary>
    public Float32x4DemoteFloat64x2Zero() { }
}
