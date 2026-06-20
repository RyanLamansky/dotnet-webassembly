using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Float32x4Pmax instruction.</summary>
public class Float32x4Pmax : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Float32x4Pmax"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float32x4Pmax;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Float32x4PmaxMethod;

    /// <summary>Creates a new <see cref="Float32x4Pmax"/> instance.</summary>
    public Float32x4Pmax() { }
}
