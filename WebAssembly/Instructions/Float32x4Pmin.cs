using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>f32x4 pseudo-min.</summary>
public class Float32x4Pmin : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Float32x4Pmin"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float32x4Pmin;

    /// <summary>Creates a new <see cref="Float32x4Pmin"/> instance.</summary>
    public Float32x4Pmin() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b)
    {
        var r = new float[4];
        for (var i = 0; i < 4; i++) { var ai = a.AsSingle().GetElement(i); var bi = b.AsSingle().GetElement(i); r[i] = bi < ai ? bi : ai; }
        return Vector128.Create(r).AsByte();
    }
}
