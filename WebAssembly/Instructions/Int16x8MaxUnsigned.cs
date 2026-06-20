using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int16x8MaxUnsigned instruction.</summary>
public class Int16x8MaxUnsigned : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8MaxUnsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8MaxUnsigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int16x8MaxUMethod;

    /// <summary>Creates a new <see cref="Int16x8MaxUnsigned"/> instance.</summary>
    public Int16x8MaxUnsigned() { }
}
