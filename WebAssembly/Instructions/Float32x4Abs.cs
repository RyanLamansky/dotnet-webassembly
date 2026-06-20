using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Float32x4Abs instruction.</summary>
public class Float32x4Abs : SimdUnaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Float32x4Abs"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float32x4Abs;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Float32x4AbsMethod;

    /// <summary>Creates a new <see cref="Float32x4Abs"/> instance.</summary>
    public Float32x4Abs() { }
}
