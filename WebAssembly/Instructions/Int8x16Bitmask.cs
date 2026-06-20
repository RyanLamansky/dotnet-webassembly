using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int8x16Bitmask instruction.</summary>
public class Int8x16Bitmask : SimdV128ToI32Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16Bitmask"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16Bitmask;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int8x16BitmaskMethod;

    /// <summary>Creates a new <see cref="Int8x16Bitmask"/> instance.</summary>
    public Int8x16Bitmask() { }
}
