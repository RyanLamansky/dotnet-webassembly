using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Float32x4NotEqual instruction.</summary>
public class Float32x4NotEqual : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Float32x4NotEqual"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float32x4NotEqual;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Float32x4NotEqualMethod;

    /// <summary>Creates a new <see cref="Float32x4NotEqual"/> instance.</summary>
    public Float32x4NotEqual() { }
}
