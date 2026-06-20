using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Float64x2Sub instruction.</summary>
public class Float64x2Sub : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Float64x2Sub"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float64x2Sub;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Float64x2SubMethod;

    /// <summary>Creates a new <see cref="Float64x2Sub"/> instance.</summary>
    public Float64x2Sub() { }
}
