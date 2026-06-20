using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int16x8NarrowInt32x4Unsigned instruction.</summary>
public class Int16x8NarrowInt32x4Unsigned : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8NarrowInt32x4Unsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8NarrowInt32x4Unsigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int16x8NarrowI32x4UMethod;

    /// <summary>Creates a new <see cref="Int16x8NarrowInt32x4Unsigned"/> instance.</summary>
    public Int16x8NarrowInt32x4Unsigned() { }
}
