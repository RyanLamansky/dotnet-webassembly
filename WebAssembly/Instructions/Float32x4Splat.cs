using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Float32x4Splat instruction.</summary>
public class Float32x4Splat : SimdSplatInstruction
{
    /// <summary>Always <see cref="SimdOpCode.Float32x4Splat"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float32x4Splat;
    internal override WebAssemblyValueType ScalarType => WebAssemblyValueType.Float32;
    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Float32x4SplatMethod;

    /// <summary>Creates a new <see cref="Float32x4Splat"/> instance.</summary>
    public Float32x4Splat() { }
}
