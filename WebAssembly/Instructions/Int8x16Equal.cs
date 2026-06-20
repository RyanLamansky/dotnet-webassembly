using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int8x16Equal instruction.</summary>
public class Int8x16Equal : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16Equal"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16Equal;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int8x16EqualMethod;

    /// <summary>Creates a new <see cref="Int8x16Equal"/> instance.</summary>
    public Int8x16Equal() { }
}
