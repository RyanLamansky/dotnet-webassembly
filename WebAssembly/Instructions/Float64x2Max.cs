using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Float64x2Max instruction.</summary>
public class Float64x2Max : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Float64x2Max"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float64x2Max;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Float64x2MaxMethod;

    /// <summary>Creates a new <see cref="Float64x2Max"/> instance.</summary>
    public Float64x2Max() { }
}
