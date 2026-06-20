using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Float32x4Sub instruction.</summary>
public class Float32x4Sub : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Float32x4Sub"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float32x4Sub;

    /// <summary>Creates a new <see cref="Float32x4Sub"/> instance.</summary>
    public Float32x4Sub() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b) => (a.AsSingle() - b.AsSingle()).AsByte();
}
