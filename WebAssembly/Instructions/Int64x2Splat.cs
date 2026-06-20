using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int64x2Splat instruction.</summary>
public class Int64x2Splat : SimdSplatInstruction
{
    /// <summary>Always <see cref="SimdOpCode.Int64x2Splat"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int64x2Splat;
    internal override WebAssemblyValueType ScalarType => WebAssemblyValueType.Int64;
    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int64x2SplatMethod;

    /// <summary>Creates a new <see cref="Int64x2Splat"/> instance.</summary>
    public Int64x2Splat() { }
}
