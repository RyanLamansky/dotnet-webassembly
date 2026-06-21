using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Splat an f64 to all f64x2 lanes.</summary>
public class Float64x2Splat : SimdSplatInstruction
{
    /// <summary>Always <see cref="SimdOpCode.Float64x2Splat"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float64x2Splat;
    internal override WebAssemblyValueType ScalarType => WebAssemblyValueType.Float64;

    /// <summary>Creates a new <see cref="Float64x2Splat"/> instance.</summary>
    public Float64x2Splat() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(double x) => Vector128.Create(x).AsByte();
}
