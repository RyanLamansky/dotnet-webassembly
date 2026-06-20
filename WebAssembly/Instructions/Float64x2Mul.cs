using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Float64x2Mul instruction.</summary>
public class Float64x2Mul : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Float64x2Mul"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float64x2Mul;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Float64x2MulMethod;

    /// <summary>Creates a new <see cref="Float64x2Mul"/> instance.</summary>
    public Float64x2Mul() { }
}
