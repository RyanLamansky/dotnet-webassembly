using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Float64x2Pmin instruction.</summary>
public class Float64x2Pmin : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Float64x2Pmin"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float64x2Pmin;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Float64x2PminMethod;

    /// <summary>Creates a new <see cref="Float64x2Pmin"/> instance.</summary>
    public Float64x2Pmin() { }
}
