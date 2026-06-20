using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Float32x4Floor instruction.</summary>
public class Float32x4Floor : SimdUnaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Float32x4Floor"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float32x4Floor;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Float32x4FloorMethod;

    /// <summary>Creates a new <see cref="Float32x4Floor"/> instance.</summary>
    public Float32x4Floor() { }
}
