using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int8x16MinSigned instruction.</summary>
public class Int8x16MinSigned : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16MinSigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16MinSigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int8x16MinSMethod;

    /// <summary>Creates a new <see cref="Int8x16MinSigned"/> instance.</summary>
    public Int8x16MinSigned() { }
}
