using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int16x8Mul instruction.</summary>
public class Int16x8Mul : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8Mul"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8Mul;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int16x8MulMethod;

    /// <summary>Creates a new <see cref="Int16x8Mul"/> instance.</summary>
    public Int16x8Mul() { }
}
