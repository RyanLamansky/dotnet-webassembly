using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int16x8LessThanOrEqualUnsigned instruction.</summary>
public class Int16x8LessThanOrEqualUnsigned : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8LessThanOrEqualUnsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8LessThanOrEqualUnsigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int16x8LeUMethod;

    /// <summary>Creates a new <see cref="Int16x8LessThanOrEqualUnsigned"/> instance.</summary>
    public Int16x8LessThanOrEqualUnsigned() { }
}
