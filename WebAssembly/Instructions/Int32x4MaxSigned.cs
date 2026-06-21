using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>i32x4 signed max.</summary>
public class Int32x4MaxSigned : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int32x4MaxSigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int32x4MaxSigned;

    /// <summary>Creates a new <see cref="Int32x4MaxSigned"/> instance.</summary>
    public Int32x4MaxSigned() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b) => Vector128.Max(a.AsInt32(), b.AsInt32()).AsByte();
}
