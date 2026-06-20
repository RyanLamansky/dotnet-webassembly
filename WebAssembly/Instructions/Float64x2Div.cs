using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Float64x2Div instruction.</summary>
public class Float64x2Div : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Float64x2Div"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float64x2Div;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Float64x2DivMethod;

    /// <summary>Creates a new <see cref="Float64x2Div"/> instance.</summary>
    public Float64x2Div() { }
}
