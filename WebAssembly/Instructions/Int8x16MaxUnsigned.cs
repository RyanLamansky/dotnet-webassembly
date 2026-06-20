using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int8x16MaxUnsigned instruction.</summary>
public class Int8x16MaxUnsigned : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16MaxUnsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16MaxUnsigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int8x16MaxUMethod;

    /// <summary>Creates a new <see cref="Int8x16MaxUnsigned"/> instance.</summary>
    public Int8x16MaxUnsigned() { }
}
