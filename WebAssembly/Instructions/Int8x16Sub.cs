using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Int8x16Sub instruction.</summary>
public class Int8x16Sub : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16Sub"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16Sub;

    /// <summary>Creates a new <see cref="Int8x16Sub"/> instance.</summary>
    public Int8x16Sub() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b) => (a.AsSByte() - b.AsSByte()).AsByte();
}
