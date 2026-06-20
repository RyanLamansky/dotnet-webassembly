using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Float64x2Equal instruction.</summary>
public class Float64x2Equal : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Float64x2Equal"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float64x2Equal;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Float64x2EqualMethod;

    /// <summary>Creates a new <see cref="Float64x2Equal"/> instance.</summary>
    public Float64x2Equal() { }
}
