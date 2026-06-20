using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Float64x2Trunc instruction.</summary>
public class Float64x2Trunc : SimdUnaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Float64x2Trunc"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float64x2Trunc;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Float64x2TruncMethod;

    /// <summary>Creates a new <see cref="Float64x2Trunc"/> instance.</summary>
    public Float64x2Trunc() { }
}
