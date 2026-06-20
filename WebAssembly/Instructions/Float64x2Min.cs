using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Float64x2Min instruction.</summary>
public class Float64x2Min : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Float64x2Min"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float64x2Min;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Float64x2MinMethod;

    /// <summary>Creates a new <see cref="Float64x2Min"/> instance.</summary>
    public Float64x2Min() { }
}
