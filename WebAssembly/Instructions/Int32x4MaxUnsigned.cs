using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int32x4MaxUnsigned instruction.</summary>
public class Int32x4MaxUnsigned : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int32x4MaxUnsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int32x4MaxUnsigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int32x4MaxUMethod;

    /// <summary>Creates a new <see cref="Int32x4MaxUnsigned"/> instance.</summary>
    public Int32x4MaxUnsigned() { }
}
