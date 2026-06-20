using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int16x8NotEqual instruction.</summary>
public class Int16x8NotEqual : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8NotEqual"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8NotEqual;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int16x8NotEqualMethod;

    /// <summary>Creates a new <see cref="Int16x8NotEqual"/> instance.</summary>
    public Int16x8NotEqual() { }
}
