using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Int8x16Abs instruction.</summary>
public class Int8x16Abs : SimdUnaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16Abs"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16Abs;

    /// <summary>Creates a new <see cref="Int8x16Abs"/> instance.</summary>
    public Int8x16Abs() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a) => Vector128.Abs(a.AsSByte()).AsByte();
}
