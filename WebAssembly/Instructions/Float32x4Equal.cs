using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Float32x4Equal instruction.</summary>
public class Float32x4Equal : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Float32x4Equal"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float32x4Equal;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Float32x4EqualMethod;

    /// <summary>Creates a new <see cref="Float32x4Equal"/> instance.</summary>
    public Float32x4Equal() { }
}
