using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int16x8LessThanOrEqualSigned instruction.</summary>
public class Int16x8LessThanOrEqualSigned : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8LessThanOrEqualSigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8LessThanOrEqualSigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int16x8LeSMethod;

    /// <summary>Creates a new <see cref="Int16x8LessThanOrEqualSigned"/> instance.</summary>
    public Int16x8LessThanOrEqualSigned() { }
}
