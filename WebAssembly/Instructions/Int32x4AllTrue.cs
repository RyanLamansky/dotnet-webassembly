using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Test whether all i32x4 lanes are non-zero.</summary>
public class Int32x4AllTrue : SimdV128ToI32Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int32x4AllTrue"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int32x4AllTrue;

    /// <summary>Creates a new <see cref="Int32x4AllTrue"/> instance.</summary>
    public Int32x4AllTrue() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static int Execute(Vector128<byte> a) => Vector128.EqualsAny(a.AsInt32(), Vector128<int>.Zero) ? 0 : 1;
}
