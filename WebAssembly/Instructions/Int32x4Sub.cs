using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int32x4Sub instruction.</summary>
public class Int32x4Sub : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int32x4Sub"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int32x4Sub;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int32x4SubMethod;

    /// <summary>Creates a new <see cref="Int32x4Sub"/> instance.</summary>
    public Int32x4Sub() { }
}
