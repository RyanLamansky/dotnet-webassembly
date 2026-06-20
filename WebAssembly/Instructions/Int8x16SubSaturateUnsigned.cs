using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int8x16SubSaturateUnsigned instruction.</summary>
public class Int8x16SubSaturateUnsigned : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16SubSaturateUnsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16SubSaturateUnsigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int8x16SubSatUMethod;

    /// <summary>Creates a new <see cref="Int8x16SubSaturateUnsigned"/> instance.</summary>
    public Int8x16SubSaturateUnsigned() { }
}
