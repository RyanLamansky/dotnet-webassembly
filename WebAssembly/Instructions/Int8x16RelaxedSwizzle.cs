using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Relaxed i8x16 swizzle; the deterministic profile is identical to <see cref="Int8x16Swizzle"/>.</summary>
public class Int8x16RelaxedSwizzle : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16RelaxedSwizzle"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16RelaxedSwizzle;

    /// <summary>Creates a new <see cref="Int8x16RelaxedSwizzle"/> instance.</summary>
    public Int8x16RelaxedSwizzle() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b) => Int8x16Swizzle.Execute(a, b);
}
