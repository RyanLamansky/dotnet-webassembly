using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Float32x4Min instruction.</summary>
public class Float32x4Min : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Float32x4Min"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float32x4Min;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Float32x4MinMethod;

    /// <summary>Creates a new <see cref="Float32x4Min"/> instance.</summary>
    public Float32x4Min() { }
}
