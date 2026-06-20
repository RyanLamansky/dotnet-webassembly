using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Float64x2Pmax instruction.</summary>
public class Float64x2Pmax : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Float64x2Pmax"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float64x2Pmax;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Float64x2PmaxMethod;

    /// <summary>Creates a new <see cref="Float64x2Pmax"/> instance.</summary>
    public Float64x2Pmax() { }
}
