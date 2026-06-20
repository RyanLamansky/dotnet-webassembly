using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Float32x4GreaterThan instruction.</summary>
public class Float32x4GreaterThan : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Float32x4GreaterThan"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float32x4GreaterThan;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Float32x4GtMethod;

    /// <summary>Creates a new <see cref="Float32x4GreaterThan"/> instance.</summary>
    public Float32x4GreaterThan() { }
}
