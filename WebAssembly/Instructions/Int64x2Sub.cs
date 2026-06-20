using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int64x2Sub instruction.</summary>
public class Int64x2Sub : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int64x2Sub"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int64x2Sub;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int64x2SubMethod;

    /// <summary>Creates a new <see cref="Int64x2Sub"/> instance.</summary>
    public Int64x2Sub() { }
}
