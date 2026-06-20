using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Int32x4Abs instruction.</summary>
public class Int32x4Abs : SimdUnaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int32x4Abs"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int32x4Abs;

    /// <summary>Creates a new <see cref="Int32x4Abs"/> instance.</summary>
    public Int32x4Abs() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a) => Vector128.Abs(a.AsInt32()).AsByte();
}
