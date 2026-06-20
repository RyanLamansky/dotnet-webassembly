using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Bitwise AND of two v128 values.</summary>
public class V128And : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.V128And"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.V128And;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.V128AndMethod;

    /// <summary>Creates a new <see cref="V128And"/> instance.</summary>
    public V128And() { }
}
