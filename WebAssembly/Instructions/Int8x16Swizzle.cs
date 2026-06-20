using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int8x16Swizzle instruction.</summary>
public class Int8x16Swizzle : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16Swizzle"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16Swizzle;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int8x16SwizzleMethod;

    /// <summary>Creates a new <see cref="Int8x16Swizzle"/> instance.</summary>
    public Int8x16Swizzle() { }
}
