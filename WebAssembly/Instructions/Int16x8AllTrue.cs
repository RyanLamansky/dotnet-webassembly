using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Test whether all i16x8 lanes are non-zero.</summary>
public class Int16x8AllTrue : SimdV128ToI32Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8AllTrue"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8AllTrue;

    /// <summary>Creates a new <see cref="Int16x8AllTrue"/> instance.</summary>
    public Int16x8AllTrue() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static int Execute(Vector128<byte> a) => Vector128.EqualsAny(a.AsInt16(), Vector128<short>.Zero) ? 0 : 1;
}
