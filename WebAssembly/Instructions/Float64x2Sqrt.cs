using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Float64x2Sqrt instruction.</summary>
public class Float64x2Sqrt : SimdUnaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Float64x2Sqrt"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float64x2Sqrt;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Float64x2SqrtMethod;

    /// <summary>Creates a new <see cref="Float64x2Sqrt"/> instance.</summary>
    public Float64x2Sqrt() { }
}
