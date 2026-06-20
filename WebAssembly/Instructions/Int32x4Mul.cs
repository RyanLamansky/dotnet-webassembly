using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int32x4Mul instruction.</summary>
public class Int32x4Mul : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int32x4Mul"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int32x4Mul;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int32x4MulMethod;

    /// <summary>Creates a new <see cref="Int32x4Mul"/> instance.</summary>
    public Int32x4Mul() { }
}
