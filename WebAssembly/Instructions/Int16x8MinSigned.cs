using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int16x8MinSigned instruction.</summary>
public class Int16x8MinSigned : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8MinSigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8MinSigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int16x8MinSMethod;

    /// <summary>Creates a new <see cref="Int16x8MinSigned"/> instance.</summary>
    public Int16x8MinSigned() { }
}
