using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int32x4Bitmask instruction.</summary>
public class Int32x4Bitmask : SimdV128ToI32Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int32x4Bitmask"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int32x4Bitmask;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int32x4BitmaskMethod;

    /// <summary>Creates a new <see cref="Int32x4Bitmask"/> instance.</summary>
    public Int32x4Bitmask() { }
}
