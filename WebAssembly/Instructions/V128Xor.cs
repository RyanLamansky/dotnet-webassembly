using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Bitwise XOR of two v128 values.</summary>
public class V128Xor : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.V128Xor"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.V128Xor;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.V128XorMethod;

    /// <summary>Creates a new <see cref="V128Xor"/> instance.</summary>
    public V128Xor() { }
}
