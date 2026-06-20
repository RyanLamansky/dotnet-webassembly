using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Float32x4Pmin instruction.</summary>
public class Float32x4Pmin : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Float32x4Pmin"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float32x4Pmin;

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.Float32x4PminMethod;

    /// <summary>Creates a new <see cref="Float32x4Pmin"/> instance.</summary>
    public Float32x4Pmin() { }
}
