using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int16x8Sub instruction.</summary>
public class Int16x8Sub : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8Sub"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8Sub;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int16x8SubMethod;

    /// <summary>Creates a new <see cref="Int16x8Sub"/> instance.</summary>
    public Int16x8Sub() { }
}
