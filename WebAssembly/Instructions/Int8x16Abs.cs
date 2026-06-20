using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int8x16Abs instruction.</summary>
public class Int8x16Abs : SimdUnaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16Abs"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16Abs;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int8x16AbsMethod;

    /// <summary>Creates a new <see cref="Int8x16Abs"/> instance.</summary>
    public Int8x16Abs() { }
}
