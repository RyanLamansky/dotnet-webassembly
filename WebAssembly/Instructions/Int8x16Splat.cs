using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int8x16Splat instruction.</summary>
public class Int8x16Splat : SimdSplatInstruction
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16Splat"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16Splat;
    internal override WebAssemblyValueType ScalarType => WebAssemblyValueType.Int32;
    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int8x16SplatMethod;

    /// <summary>Creates a new <see cref="Int8x16Splat"/> instance.</summary>
    public Int8x16Splat() { }
}
