using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Float64x2GreaterThan instruction.</summary>
public class Float64x2GreaterThan : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Float64x2GreaterThan"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float64x2GreaterThan;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Float64x2GtMethod;

    /// <summary>Creates a new <see cref="Float64x2GreaterThan"/> instance.</summary>
    public Float64x2GreaterThan() { }
}
