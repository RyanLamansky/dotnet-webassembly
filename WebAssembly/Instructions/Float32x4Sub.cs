using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Float32x4Sub instruction.</summary>
public class Float32x4Sub : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Float32x4Sub"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float32x4Sub;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Float32x4SubMethod;

    /// <summary>Creates a new <see cref="Float32x4Sub"/> instance.</summary>
    public Float32x4Sub() { }
}
