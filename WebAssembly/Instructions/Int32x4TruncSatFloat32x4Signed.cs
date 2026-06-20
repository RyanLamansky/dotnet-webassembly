using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int32x4TruncSatFloat32x4Signed instruction.</summary>
public class Int32x4TruncSatFloat32x4Signed : SimdUnaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int32x4TruncSatFloat32x4Signed"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int32x4TruncSatFloat32x4Signed;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int32x4TruncSatF32x4SMethod;

    /// <summary>Creates a new <see cref="Int32x4TruncSatFloat32x4Signed"/> instance.</summary>
    public Int32x4TruncSatFloat32x4Signed() { }
}
