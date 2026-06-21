using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>i32x4 signed min.</summary>
public class Int32x4MinSigned : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int32x4MinSigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int32x4MinSigned;

    /// <summary>Creates a new <see cref="Int32x4MinSigned"/> instance.</summary>
    public Int32x4MinSigned() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b) => Vector128.Min(a.AsInt32(), b.AsInt32()).AsByte();
}
