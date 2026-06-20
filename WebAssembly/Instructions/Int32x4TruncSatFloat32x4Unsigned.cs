using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int32x4TruncSatFloat32x4Unsigned instruction.</summary>
public class Int32x4TruncSatFloat32x4Unsigned : SimdUnaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int32x4TruncSatFloat32x4Unsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int32x4TruncSatFloat32x4Unsigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int32x4TruncSatF32x4UMethod;

    /// <summary>Creates a new <see cref="Int32x4TruncSatFloat32x4Unsigned"/> instance.</summary>
    public Int32x4TruncSatFloat32x4Unsigned() { }
}
