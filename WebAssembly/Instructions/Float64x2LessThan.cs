using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Float64x2LessThan instruction.</summary>
public class Float64x2LessThan : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Float64x2LessThan"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float64x2LessThan;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Float64x2LtMethod;

    /// <summary>Creates a new <see cref="Float64x2LessThan"/> instance.</summary>
    public Float64x2LessThan() { }
}
