using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Int16x8Splat instruction.</summary>
public class Int16x8Splat : SimdSplatInstruction
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8Splat"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8Splat;
    internal override WebAssemblyValueType ScalarType => WebAssemblyValueType.Int32;
    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Int16x8SplatMethod;

    /// <summary>Creates a new <see cref="Int16x8Splat"/> instance.</summary>
    public Int16x8Splat() { }
}
