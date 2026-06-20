using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int8x16NarrowInt16x8Unsigned instruction.</summary>
public class Int8x16NarrowInt16x8Unsigned : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16NarrowInt16x8Unsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16NarrowInt16x8Unsigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int8x16NarrowI16x8UMethod;

    /// <summary>Creates a new <see cref="Int8x16NarrowInt16x8Unsigned"/> instance.</summary>
    public Int8x16NarrowInt16x8Unsigned() { }
}
