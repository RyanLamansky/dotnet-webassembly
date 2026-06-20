using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int32x4LessThanSigned instruction.</summary>
public class Int32x4LessThanSigned : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int32x4LessThanSigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int32x4LessThanSigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int32x4LtSMethod;

    /// <summary>Creates a new <see cref="Int32x4LessThanSigned"/> instance.</summary>
    public Int32x4LessThanSigned() { }
}
