using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int32x4Equal instruction.</summary>
public class Int32x4Equal : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int32x4Equal"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int32x4Equal;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int32x4EqualMethod;

    /// <summary>Creates a new <see cref="Int32x4Equal"/> instance.</summary>
    public Int32x4Equal() { }
}
