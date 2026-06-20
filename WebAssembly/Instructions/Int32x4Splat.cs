using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int32x4Splat instruction.</summary>
public class Int32x4Splat : SimdSplatInstruction
{
    /// <summary>Always <see cref="SimdOpCode.Int32x4Splat"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int32x4Splat;
    internal override WebAssemblyValueType ScalarType => WebAssemblyValueType.Int32;
    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int32x4SplatMethod;

    /// <summary>Creates a new <see cref="Int32x4Splat"/> instance.</summary>
    public Int32x4Splat() { }
}
