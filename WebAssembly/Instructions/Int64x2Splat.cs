using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Int64x2Splat instruction.</summary>
public class Int64x2Splat : SimdSplatInstruction
{
    /// <summary>Always <see cref="SimdOpCode.Int64x2Splat"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int64x2Splat;
    internal override WebAssemblyValueType ScalarType => WebAssemblyValueType.Int64;

    /// <summary>Creates a new <see cref="Int64x2Splat"/> instance.</summary>
    public Int64x2Splat() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(long x) => Vector128.Create(x).AsByte();
}
