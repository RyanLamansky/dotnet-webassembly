using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Count non-zero bits in each i8x16 lane.</summary>
public class Int8x16Popcnt : SimdUnaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16Popcnt"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16Popcnt;

    /// <summary>Creates a new <see cref="Int8x16Popcnt"/> instance.</summary>
    public Int8x16Popcnt() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a) { var r = new byte[16]; for (var i = 0; i < 16; i++) r[i] = (byte)System.Numerics.BitOperations.PopCount(a.GetElement(i)); return Vector128.Create(r); }
}
