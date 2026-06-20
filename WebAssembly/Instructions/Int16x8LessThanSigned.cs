using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int16x8LessThanSigned instruction.</summary>
public class Int16x8LessThanSigned : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8LessThanSigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8LessThanSigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int16x8LtSMethod;

    /// <summary>Creates a new <see cref="Int16x8LessThanSigned"/> instance.</summary>
    public Int16x8LessThanSigned() { }
}
