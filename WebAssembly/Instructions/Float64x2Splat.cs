using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Float64x2Splat instruction.</summary>
public class Float64x2Splat : SimdSplatInstruction
{
    /// <summary>Always <see cref="SimdOpCode.Float64x2Splat"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float64x2Splat;
    internal override WebAssemblyValueType ScalarType => WebAssemblyValueType.Float64;
    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Float64x2SplatMethod;

    /// <summary>Creates a new <see cref="Float64x2Splat"/> instance.</summary>
    public Float64x2Splat() { }
}
