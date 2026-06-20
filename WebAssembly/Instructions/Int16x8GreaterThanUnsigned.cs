using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int16x8GreaterThanUnsigned instruction.</summary>
public class Int16x8GreaterThanUnsigned : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8GreaterThanUnsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8GreaterThanUnsigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int16x8GtUMethod;

    /// <summary>Creates a new <see cref="Int16x8GreaterThanUnsigned"/> instance.</summary>
    public Int16x8GreaterThanUnsigned() { }
}
