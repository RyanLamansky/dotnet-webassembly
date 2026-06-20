using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int16x8Abs instruction.</summary>
public class Int16x8Abs : SimdUnaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8Abs"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8Abs;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int16x8AbsMethod;

    /// <summary>Creates a new <see cref="Int16x8Abs"/> instance.</summary>
    public Int16x8Abs() { }
}
