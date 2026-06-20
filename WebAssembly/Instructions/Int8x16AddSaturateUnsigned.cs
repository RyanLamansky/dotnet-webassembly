using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int8x16AddSaturateUnsigned instruction.</summary>
public class Int8x16AddSaturateUnsigned : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16AddSaturateUnsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16AddSaturateUnsigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int8x16AddSatUMethod;

    /// <summary>Creates a new <see cref="Int8x16AddSaturateUnsigned"/> instance.</summary>
    public Int8x16AddSaturateUnsigned() { }
}
