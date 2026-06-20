using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int64x2Mul instruction.</summary>
public class Int64x2Mul : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int64x2Mul"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int64x2Mul;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int64x2MulMethod;

    /// <summary>Creates a new <see cref="Int64x2Mul"/> instance.</summary>
    public Int64x2Mul() { }
}
