using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int32x4LessThanOrEqualUnsigned instruction.</summary>
public class Int32x4LessThanOrEqualUnsigned : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int32x4LessThanOrEqualUnsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int32x4LessThanOrEqualUnsigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int32x4LeUMethod;

    /// <summary>Creates a new <see cref="Int32x4LessThanOrEqualUnsigned"/> instance.</summary>
    public Int32x4LessThanOrEqualUnsigned() { }
}
