using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int64x2Equal instruction.</summary>
public class Int64x2Equal : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int64x2Equal"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int64x2Equal;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int64x2EqualMethod;

    /// <summary>Creates a new <see cref="Int64x2Equal"/> instance.</summary>
    public Int64x2Equal() { }
}
