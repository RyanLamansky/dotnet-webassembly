using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Int16x8GreaterThanOrEqualSigned instruction.</summary>
public class Int16x8GreaterThanOrEqualSigned : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8GreaterThanOrEqualSigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8GreaterThanOrEqualSigned;

    /// <summary>Creates a new <see cref="Int16x8GreaterThanOrEqualSigned"/> instance.</summary>
    public Int16x8GreaterThanOrEqualSigned() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b) => Vector128.GreaterThanOrEqual(a.AsInt16(), b.AsInt16()).AsByte();
}
