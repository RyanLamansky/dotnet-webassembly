using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Float64x2Neg instruction.</summary>
public class Float64x2Neg : SimdUnaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Float64x2Neg"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float64x2Neg;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Float64x2NegMethod;

    /// <summary>Creates a new <see cref="Float64x2Neg"/> instance.</summary>
    public Float64x2Neg() { }
}
