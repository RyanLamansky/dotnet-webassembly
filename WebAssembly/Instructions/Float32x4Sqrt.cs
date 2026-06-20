using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Float32x4Sqrt instruction.</summary>
public class Float32x4Sqrt : SimdUnaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Float32x4Sqrt"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float32x4Sqrt;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Float32x4SqrtMethod;

    /// <summary>Creates a new <see cref="Float32x4Sqrt"/> instance.</summary>
    public Float32x4Sqrt() { }
}
