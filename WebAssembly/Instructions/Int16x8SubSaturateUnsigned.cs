using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int16x8SubSaturateUnsigned instruction.</summary>
public class Int16x8SubSaturateUnsigned : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8SubSaturateUnsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8SubSaturateUnsigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int16x8SubSatUMethod;

    /// <summary>Creates a new <see cref="Int16x8SubSaturateUnsigned"/> instance.</summary>
    public Int16x8SubSaturateUnsigned() { }
}
