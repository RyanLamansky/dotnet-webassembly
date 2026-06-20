using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Float64x2Floor instruction.</summary>
public class Float64x2Floor : SimdUnaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Float64x2Floor"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float64x2Floor;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Float64x2FloorMethod;

    /// <summary>Creates a new <see cref="Float64x2Floor"/> instance.</summary>
    public Float64x2Floor() { }
}
