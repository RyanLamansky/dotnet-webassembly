using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int16x8Equal instruction.</summary>
public class Int16x8Equal : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8Equal"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8Equal;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int16x8EqualMethod;

    /// <summary>Creates a new <see cref="Int16x8Equal"/> instance.</summary>
    public Int16x8Equal() { }
}
