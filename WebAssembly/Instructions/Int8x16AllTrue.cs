using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Test whether all i8x16 lanes are non-zero.</summary>
public class Int8x16AllTrue : SimdV128ToI32Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16AllTrue"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16AllTrue;

    /// <summary>Creates a new <see cref="Int8x16AllTrue"/> instance.</summary>
    public Int8x16AllTrue() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static int Execute(Vector128<byte> a) => Vector128.EqualsAny(a, Vector128<byte>.Zero) ? 0 : 1;
}
