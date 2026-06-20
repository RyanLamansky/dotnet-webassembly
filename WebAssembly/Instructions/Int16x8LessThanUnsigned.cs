using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int16x8LessThanUnsigned instruction.</summary>
public class Int16x8LessThanUnsigned : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8LessThanUnsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8LessThanUnsigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int16x8LtUMethod;

    /// <summary>Creates a new <see cref="Int16x8LessThanUnsigned"/> instance.</summary>
    public Int16x8LessThanUnsigned() { }
}
