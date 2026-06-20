using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int8x16LessThanOrEqualUnsigned instruction.</summary>
public class Int8x16LessThanOrEqualUnsigned : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16LessThanOrEqualUnsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16LessThanOrEqualUnsigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int8x16LeUMethod;

    /// <summary>Creates a new <see cref="Int8x16LessThanOrEqualUnsigned"/> instance.</summary>
    public Int8x16LessThanOrEqualUnsigned() { }
}
