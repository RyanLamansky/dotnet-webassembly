using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int64x2Abs instruction.</summary>
public class Int64x2Abs : SimdUnaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int64x2Abs"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int64x2Abs;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int64x2AbsMethod;

    /// <summary>Creates a new <see cref="Int64x2Abs"/> instance.</summary>
    public Int64x2Abs() { }
}
