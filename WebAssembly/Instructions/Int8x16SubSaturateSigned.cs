using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int8x16SubSaturateSigned instruction.</summary>
public class Int8x16SubSaturateSigned : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16SubSaturateSigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16SubSaturateSigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int8x16SubSatSMethod;

    /// <summary>Creates a new <see cref="Int8x16SubSaturateSigned"/> instance.</summary>
    public Int8x16SubSaturateSigned() { }
}
