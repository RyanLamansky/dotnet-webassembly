using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int32x4GreaterThanUnsigned instruction.</summary>
public class Int32x4GreaterThanUnsigned : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int32x4GreaterThanUnsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int32x4GreaterThanUnsigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int32x4GtUMethod;

    /// <summary>Creates a new <see cref="Int32x4GreaterThanUnsigned"/> instance.</summary>
    public Int32x4GreaterThanUnsigned() { }
}
