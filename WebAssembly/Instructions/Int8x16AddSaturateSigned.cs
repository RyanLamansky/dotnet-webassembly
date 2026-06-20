using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int8x16AddSaturateSigned instruction.</summary>
public class Int8x16AddSaturateSigned : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16AddSaturateSigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16AddSaturateSigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int8x16AddSatSMethod;

    /// <summary>Creates a new <see cref="Int8x16AddSaturateSigned"/> instance.</summary>
    public Int8x16AddSaturateSigned() { }
}
