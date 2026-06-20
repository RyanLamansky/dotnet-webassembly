using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Float32x4Splat instruction.</summary>
public class Float32x4Splat : SimdSplatInstruction
{
    /// <summary>Always <see cref="SimdOpCode.Float32x4Splat"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float32x4Splat;
    internal override WebAssemblyValueType ScalarType => WebAssemblyValueType.Float32;

    /// <summary>Creates a new <see cref="Float32x4Splat"/> instance.</summary>
    public Float32x4Splat() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(float x) => Vector128.Create(x).AsByte();
}
