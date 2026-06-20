using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int8x16NotEqual instruction.</summary>
public class Int8x16NotEqual : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16NotEqual"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16NotEqual;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int8x16NotEqualMethod;

    /// <summary>Creates a new <see cref="Int8x16NotEqual"/> instance.</summary>
    public Int8x16NotEqual() { }
}
