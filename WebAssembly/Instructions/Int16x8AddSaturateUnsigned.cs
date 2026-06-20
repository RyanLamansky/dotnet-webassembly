using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int16x8AddSaturateUnsigned instruction.</summary>
public class Int16x8AddSaturateUnsigned : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8AddSaturateUnsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8AddSaturateUnsigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int16x8AddSatUMethod;

    /// <summary>Creates a new <see cref="Int16x8AddSaturateUnsigned"/> instance.</summary>
    public Int16x8AddSaturateUnsigned() { }
}
