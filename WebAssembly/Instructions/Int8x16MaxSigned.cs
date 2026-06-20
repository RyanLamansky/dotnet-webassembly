using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int8x16MaxSigned instruction.</summary>
public class Int8x16MaxSigned : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16MaxSigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16MaxSigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int8x16MaxSMethod;

    /// <summary>Creates a new <see cref="Int8x16MaxSigned"/> instance.</summary>
    public Int8x16MaxSigned() { }
}
