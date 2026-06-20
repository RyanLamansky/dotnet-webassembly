using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int16x8GreaterThanOrEqualUnsigned instruction.</summary>
public class Int16x8GreaterThanOrEqualUnsigned : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8GreaterThanOrEqualUnsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8GreaterThanOrEqualUnsigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int16x8GeUMethod;

    /// <summary>Creates a new <see cref="Int16x8GreaterThanOrEqualUnsigned"/> instance.</summary>
    public Int16x8GreaterThanOrEqualUnsigned() { }
}
