using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int32x4GreaterThanOrEqualUnsigned instruction.</summary>
public class Int32x4GreaterThanOrEqualUnsigned : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int32x4GreaterThanOrEqualUnsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int32x4GreaterThanOrEqualUnsigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int32x4GeUMethod;

    /// <summary>Creates a new <see cref="Int32x4GreaterThanOrEqualUnsigned"/> instance.</summary>
    public Int32x4GreaterThanOrEqualUnsigned() { }
}
