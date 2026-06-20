using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Int32x4DotInt16x8Signed instruction.</summary>
public class Int32x4DotInt16x8Signed : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int32x4DotInt16x8Signed"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int32x4DotInt16x8Signed;

    /// <summary>Creates a new <see cref="Int32x4DotInt16x8Signed"/> instance.</summary>
    public Int32x4DotInt16x8Signed() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b) { var r = new int[4]; for (var i = 0; i < 4; i++) r[i] = a.AsInt16().GetElement(i*2) * b.AsInt16().GetElement(i*2) + a.AsInt16().GetElement(i*2+1) * b.AsInt16().GetElement(i*2+1); return Vector128.Create(r).AsByte(); }
}
