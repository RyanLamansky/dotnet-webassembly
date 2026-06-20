using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int64x2ExtmulHighInt32x4Signed instruction.</summary>
public class Int64x2ExtmulHighInt32x4Signed : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int64x2ExtmulHighInt32x4Signed"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int64x2ExtmulHighInt32x4Signed;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int64x2ExtmulHighI32x4SMethod;

    /// <summary>Creates a new <see cref="Int64x2ExtmulHighInt32x4Signed"/> instance.</summary>
    public Int64x2ExtmulHighInt32x4Signed() { }
}
