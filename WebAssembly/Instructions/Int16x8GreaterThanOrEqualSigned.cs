using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int16x8GreaterThanOrEqualSigned instruction.</summary>
public class Int16x8GreaterThanOrEqualSigned : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8GreaterThanOrEqualSigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8GreaterThanOrEqualSigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int16x8GeSMethod;

    /// <summary>Creates a new <see cref="Int16x8GreaterThanOrEqualSigned"/> instance.</summary>
    public Int16x8GreaterThanOrEqualSigned() { }
}
