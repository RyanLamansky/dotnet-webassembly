using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Float32x4LessThanOrEqual instruction.</summary>
public class Float32x4LessThanOrEqual : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Float32x4LessThanOrEqual"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float32x4LessThanOrEqual;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Float32x4LeMethod;

    /// <summary>Creates a new <see cref="Float32x4LessThanOrEqual"/> instance.</summary>
    public Float32x4LessThanOrEqual() { }
}
