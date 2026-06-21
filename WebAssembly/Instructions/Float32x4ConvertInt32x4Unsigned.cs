using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Convert i32x4 to f32x4, unsigned.</summary>
public class Float32x4ConvertInt32x4Unsigned : SimdUnaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Float32x4ConvertInt32x4Unsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Float32x4ConvertInt32x4Unsigned;

    /// <summary>Creates a new <see cref="Float32x4ConvertInt32x4Unsigned"/> instance.</summary>
    public Float32x4ConvertInt32x4Unsigned() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a) { var r = new float[4]; for (var i = 0; i < 4; i++) r[i] = a.AsUInt32().GetElement(i); return Vector128.Create(r).AsByte(); }
}
