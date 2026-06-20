using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int16x8GreaterThanSigned instruction.</summary>
public class Int16x8GreaterThanSigned : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8GreaterThanSigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8GreaterThanSigned;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int16x8GtSMethod;

    /// <summary>Creates a new <see cref="Int16x8GreaterThanSigned"/> instance.</summary>
    public Int16x8GreaterThanSigned() { }
}
