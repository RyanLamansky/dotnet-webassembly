using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Splat an i32 to all i8x16 lanes.</summary>
public class Int8x16Splat : SimdSplatInstruction
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16Splat"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16Splat;
    internal override WebAssemblyValueType ScalarType => WebAssemblyValueType.Int32;

    /// <summary>Creates a new <see cref="Int8x16Splat"/> instance.</summary>
    public Int8x16Splat() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(int x) => Vector128.Create((byte)(x & 0xFF));
}
