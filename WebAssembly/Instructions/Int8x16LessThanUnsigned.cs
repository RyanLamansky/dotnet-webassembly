using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int8x16LessThanUnsigned instruction.</summary>
public class Int8x16LessThanUnsigned : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16LessThanUnsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16LessThanUnsigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int8x16LtUMethod;

    /// <summary>Creates a new <see cref="Int8x16LessThanUnsigned"/> instance.</summary>
    public Int8x16LessThanUnsigned() { }
}
