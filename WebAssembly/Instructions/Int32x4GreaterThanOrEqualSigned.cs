using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int32x4GreaterThanOrEqualSigned instruction.</summary>
public class Int32x4GreaterThanOrEqualSigned : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int32x4GreaterThanOrEqualSigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int32x4GreaterThanOrEqualSigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int32x4GeSMethod;

    /// <summary>Creates a new <see cref="Int32x4GreaterThanOrEqualSigned"/> instance.</summary>
    public Int32x4GreaterThanOrEqualSigned() { }
}
