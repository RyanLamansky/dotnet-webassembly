using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int32x4LessThanOrEqualSigned instruction.</summary>
public class Int32x4LessThanOrEqualSigned : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int32x4LessThanOrEqualSigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int32x4LessThanOrEqualSigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int32x4LeSMethod;

    /// <summary>Creates a new <see cref="Int32x4LessThanOrEqualSigned"/> instance.</summary>
    public Int32x4LessThanOrEqualSigned() { }
}
