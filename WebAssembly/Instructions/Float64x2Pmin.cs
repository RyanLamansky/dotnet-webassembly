using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Float64x2Pmin instruction.</summary>
public class Float64x2Pmin : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Float64x2Pmin"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float64x2Pmin;

    /// <summary>Creates a new <see cref="Float64x2Pmin"/> instance.</summary>
    public Float64x2Pmin() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b)
    {
        var r = new double[2];
        for (var i = 0; i < 2; i++) { var ai = a.AsDouble().GetElement(i); var bi = b.AsDouble().GetElement(i); r[i] = bi < ai ? bi : ai; }
        return Vector128.Create(r).AsByte();
    }
}
