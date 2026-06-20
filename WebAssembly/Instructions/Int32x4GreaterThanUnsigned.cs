using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Int32x4GreaterThanUnsigned instruction.</summary>
public class Int32x4GreaterThanUnsigned : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int32x4GreaterThanUnsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int32x4GreaterThanUnsigned;

    /// <summary>Creates a new <see cref="Int32x4GreaterThanUnsigned"/> instance.</summary>
    public Int32x4GreaterThanUnsigned() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b) => Vector128.GreaterThan(a.AsUInt32(), b.AsUInt32()).AsByte();
}
