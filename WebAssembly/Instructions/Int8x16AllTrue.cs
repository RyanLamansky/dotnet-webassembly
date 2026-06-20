using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int8x16AllTrue instruction.</summary>
public class Int8x16AllTrue : SimdV128ToI32Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16AllTrue"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16AllTrue;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int8x16AllTrueMethod;

    /// <summary>Creates a new <see cref="Int8x16AllTrue"/> instance.</summary>
    public Int8x16AllTrue() { }
}
