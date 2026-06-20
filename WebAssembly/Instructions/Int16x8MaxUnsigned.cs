using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Int16x8MaxUnsigned instruction.</summary>
public class Int16x8MaxUnsigned : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8MaxUnsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8MaxUnsigned;

    /// <summary>Creates a new <see cref="Int16x8MaxUnsigned"/> instance.</summary>
    public Int16x8MaxUnsigned() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b) => Vector128.Max(a.AsUInt16(), b.AsUInt16()).AsByte();
}
