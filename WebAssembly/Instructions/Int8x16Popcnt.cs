using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int8x16Popcnt instruction.</summary>
public class Int8x16Popcnt : SimdUnaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16Popcnt"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16Popcnt;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int8x16PopcntMethod;

    /// <summary>Creates a new <see cref="Int8x16Popcnt"/> instance.</summary>
    public Int8x16Popcnt() { }
}
