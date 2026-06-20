using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Int32x4MinUnsigned instruction.</summary>
public class Int32x4MinUnsigned : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int32x4MinUnsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int32x4MinUnsigned;

    /// <summary>Creates a new <see cref="Int32x4MinUnsigned"/> instance.</summary>
    public Int32x4MinUnsigned() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b) => Vector128.Min(a.AsUInt32(), b.AsUInt32()).AsByte();
}
