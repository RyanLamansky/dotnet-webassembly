using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Int32x4Splat instruction.</summary>
public class Int32x4Splat : SimdSplatInstruction
{
    /// <summary>Always <see cref="SimdOpCode.Int32x4Splat"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int32x4Splat;
    internal override WebAssemblyValueType ScalarType => WebAssemblyValueType.Int32;

    /// <summary>Creates a new <see cref="Int32x4Splat"/> instance.</summary>
    public Int32x4Splat() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(int x) => Vector128.Create(x).AsByte();
}
