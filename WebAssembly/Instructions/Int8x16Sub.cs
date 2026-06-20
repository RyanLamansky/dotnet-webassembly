using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int8x16Sub instruction.</summary>
public class Int8x16Sub : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16Sub"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16Sub;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int8x16SubMethod;

    /// <summary>Creates a new <see cref="Int8x16Sub"/> instance.</summary>
    public Int8x16Sub() { }
}
