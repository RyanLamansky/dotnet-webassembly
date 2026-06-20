using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Bitwise NOT of a v128 value.</summary>
public class V128Not : SimdUnaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.V128Not"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.V128Not;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.V128NotMethod;

    /// <summary>Creates a new <see cref="V128Not"/> instance.</summary>
    public V128Not() { }
}
