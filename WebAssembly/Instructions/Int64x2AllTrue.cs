using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Int64x2AllTrue instruction.</summary>
public class Int64x2AllTrue : SimdV128ToI32Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int64x2AllTrue"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int64x2AllTrue;

    /// <summary>Creates a new <see cref="Int64x2AllTrue"/> instance.</summary>
    public Int64x2AllTrue() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static int Execute(Vector128<byte> a) => Vector128.EqualsAny(a.AsInt64(), Vector128<long>.Zero) ? 0 : 1;
}
