using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Int32x4LessThanOrEqualUnsigned instruction.</summary>
public class Int32x4LessThanOrEqualUnsigned : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int32x4LessThanOrEqualUnsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int32x4LessThanOrEqualUnsigned;

    /// <summary>Creates a new <see cref="Int32x4LessThanOrEqualUnsigned"/> instance.</summary>
    public Int32x4LessThanOrEqualUnsigned() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b) => Vector128.LessThanOrEqual(a.AsUInt32(), b.AsUInt32()).AsByte();
}
