using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int16x8Bitmask instruction.</summary>
public class Int16x8Bitmask : SimdV128ToI32Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8Bitmask"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8Bitmask;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int16x8BitmaskMethod;

    /// <summary>Creates a new <see cref="Int16x8Bitmask"/> instance.</summary>
    public Int16x8Bitmask() { }
}
