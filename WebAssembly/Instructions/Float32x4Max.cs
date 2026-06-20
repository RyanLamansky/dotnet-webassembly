using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Float32x4Max instruction.</summary>
public class Float32x4Max : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Float32x4Max"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float32x4Max;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Float32x4MaxMethod;

    /// <summary>Creates a new <see cref="Float32x4Max"/> instance.</summary>
    public Float32x4Max() { }
}
