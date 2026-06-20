using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int64x2LessThanSigned instruction.</summary>
public class Int64x2LessThanSigned : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int64x2LessThanSigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int64x2LessThanSigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int64x2LtSMethod;

    /// <summary>Creates a new <see cref="Int64x2LessThanSigned"/> instance.</summary>
    public Int64x2LessThanSigned() { }
}
