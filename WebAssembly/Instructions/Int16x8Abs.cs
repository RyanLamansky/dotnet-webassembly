using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>i16x8 absolute value.</summary>
public class Int16x8Abs : SimdUnaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8Abs"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8Abs;

    /// <summary>Creates a new <see cref="Int16x8Abs"/> instance.</summary>
    public Int16x8Abs() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a) => Vector128.Abs(a.AsInt16()).AsByte();
}
