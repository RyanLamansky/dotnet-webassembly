using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int16x8AllTrue instruction.</summary>
public class Int16x8AllTrue : SimdV128ToI32Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8AllTrue"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8AllTrue;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int16x8AllTrueMethod;

    /// <summary>Creates a new <see cref="Int16x8AllTrue"/> instance.</summary>
    public Int16x8AllTrue() { }
}
