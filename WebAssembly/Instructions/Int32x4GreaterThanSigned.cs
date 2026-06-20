using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int32x4GreaterThanSigned instruction.</summary>
public class Int32x4GreaterThanSigned : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int32x4GreaterThanSigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int32x4GreaterThanSigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int32x4GtSMethod;

    /// <summary>Creates a new <see cref="Int32x4GreaterThanSigned"/> instance.</summary>
    public Int32x4GreaterThanSigned() { }
}
