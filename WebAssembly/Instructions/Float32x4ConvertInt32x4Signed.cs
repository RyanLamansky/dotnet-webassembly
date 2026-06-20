using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Float32x4ConvertInt32x4Signed instruction.</summary>
public class Float32x4ConvertInt32x4Signed : SimdUnaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Float32x4ConvertInt32x4Signed"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float32x4ConvertInt32x4Signed;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Float32x4ConvertI32x4SMethod;

    /// <summary>Creates a new <see cref="Float32x4ConvertInt32x4Signed"/> instance.</summary>
    public Float32x4ConvertInt32x4Signed() { }
}
