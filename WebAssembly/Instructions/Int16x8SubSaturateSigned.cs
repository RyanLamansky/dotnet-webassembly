using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int16x8SubSaturateSigned instruction.</summary>
public class Int16x8SubSaturateSigned : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8SubSaturateSigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8SubSaturateSigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int16x8SubSatSMethod;

    /// <summary>Creates a new <see cref="Int16x8SubSaturateSigned"/> instance.</summary>
    public Int16x8SubSaturateSigned() { }
}
