using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Float32x4Mul instruction.</summary>
public class Float32x4Mul : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Float32x4Mul"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float32x4Mul;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Float32x4MulMethod;

    /// <summary>Creates a new <see cref="Float32x4Mul"/> instance.</summary>
    public Float32x4Mul() { }
}
