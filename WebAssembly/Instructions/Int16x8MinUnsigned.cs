using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int16x8MinUnsigned instruction.</summary>
public class Int16x8MinUnsigned : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8MinUnsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8MinUnsigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int16x8MinUMethod;

    /// <summary>Creates a new <see cref="Int16x8MinUnsigned"/> instance.</summary>
    public Int16x8MinUnsigned() { }
}
