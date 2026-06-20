using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int64x2GreaterThanSigned instruction.</summary>
public class Int64x2GreaterThanSigned : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int64x2GreaterThanSigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int64x2GreaterThanSigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int64x2GtSMethod;

    /// <summary>Creates a new <see cref="Int64x2GreaterThanSigned"/> instance.</summary>
    public Int64x2GreaterThanSigned() { }
}
