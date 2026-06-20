using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int8x16LessThanOrEqualSigned instruction.</summary>
public class Int8x16LessThanOrEqualSigned : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16LessThanOrEqualSigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16LessThanOrEqualSigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int8x16LeSMethod;

    /// <summary>Creates a new <see cref="Int8x16LessThanOrEqualSigned"/> instance.</summary>
    public Int8x16LessThanOrEqualSigned() { }
}
