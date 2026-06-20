using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int32x4Neg instruction.</summary>
public class Int32x4Neg : SimdUnaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int32x4Neg"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int32x4Neg;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int32x4NegMethod;

    /// <summary>Creates a new <see cref="Int32x4Neg"/> instance.</summary>
    public Int32x4Neg() { }
}
