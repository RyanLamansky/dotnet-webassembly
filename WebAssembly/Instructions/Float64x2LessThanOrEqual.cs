using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Float64x2LessThanOrEqual instruction.</summary>
public class Float64x2LessThanOrEqual : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Float64x2LessThanOrEqual"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float64x2LessThanOrEqual;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Float64x2LeMethod;

    /// <summary>Creates a new <see cref="Float64x2LessThanOrEqual"/> instance.</summary>
    public Float64x2LessThanOrEqual() { }
}
