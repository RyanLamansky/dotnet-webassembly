using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int8x16NarrowInt16x8Signed instruction.</summary>
public class Int8x16NarrowInt16x8Signed : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16NarrowInt16x8Signed"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16NarrowInt16x8Signed;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int8x16NarrowI16x8SMethod;

    /// <summary>Creates a new <see cref="Int8x16NarrowInt16x8Signed"/> instance.</summary>
    public Int8x16NarrowInt16x8Signed() { }
}
