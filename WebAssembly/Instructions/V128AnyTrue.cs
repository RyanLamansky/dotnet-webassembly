using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>V128AnyTrue instruction.</summary>
public class V128AnyTrue : SimdV128ToI32Instruction
{
    /// <summary>Always <see cref="SimdOpCode.V128AnyTrue"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.V128AnyTrue;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.V128AnyTrueMethod;

    /// <summary>Creates a new <see cref="V128AnyTrue"/> instance.</summary>
    public V128AnyTrue() { }
}
