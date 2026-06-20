using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int64x2GreaterThanOrEqualSigned instruction.</summary>
public class Int64x2GreaterThanOrEqualSigned : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int64x2GreaterThanOrEqualSigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int64x2GreaterThanOrEqualSigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int64x2GeSMethod;

    /// <summary>Creates a new <see cref="Int64x2GreaterThanOrEqualSigned"/> instance.</summary>
    public Int64x2GreaterThanOrEqualSigned() { }
}
