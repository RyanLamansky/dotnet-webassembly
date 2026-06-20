using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Float64x2Abs instruction.</summary>
public class Float64x2Abs : SimdUnaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Float64x2Abs"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float64x2Abs;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Float64x2AbsMethod;

    /// <summary>Creates a new <see cref="Float64x2Abs"/> instance.</summary>
    public Float64x2Abs() { }
}
