using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int8x16Neg instruction.</summary>
public class Int8x16Neg : SimdUnaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16Neg"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16Neg;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int8x16NegMethod;

    /// <summary>Creates a new <see cref="Int8x16Neg"/> instance.</summary>
    public Int8x16Neg() { }
}
