using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int32x4AllTrue instruction.</summary>
public class Int32x4AllTrue : SimdV128ToI32Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int32x4AllTrue"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int32x4AllTrue;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int32x4AllTrueMethod;

    /// <summary>Creates a new <see cref="Int32x4AllTrue"/> instance.</summary>
    public Int32x4AllTrue() { }
}
