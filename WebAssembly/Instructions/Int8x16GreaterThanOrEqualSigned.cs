using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int8x16GreaterThanOrEqualSigned instruction.</summary>
public class Int8x16GreaterThanOrEqualSigned : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16GreaterThanOrEqualSigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16GreaterThanOrEqualSigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int8x16GeSMethod;

    /// <summary>Creates a new <see cref="Int8x16GreaterThanOrEqualSigned"/> instance.</summary>
    public Int8x16GreaterThanOrEqualSigned() { }
}
