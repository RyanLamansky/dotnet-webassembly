using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>i32x4 multiply.</summary>
public class Int32x4Mul : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int32x4Mul"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int32x4Mul;

    /// <summary>Creates a new <see cref="Int32x4Mul"/> instance.</summary>
    public Int32x4Mul() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b) => (a.AsInt32() * b.AsInt32()).AsByte();
}
