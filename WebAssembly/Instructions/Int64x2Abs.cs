using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Int64x2Abs instruction.</summary>
public class Int64x2Abs : SimdUnaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int64x2Abs"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int64x2Abs;

    /// <summary>Creates a new <see cref="Int64x2Abs"/> instance.</summary>
    public Int64x2Abs() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a) => Vector128.Abs(a.AsInt64()).AsByte();
}
