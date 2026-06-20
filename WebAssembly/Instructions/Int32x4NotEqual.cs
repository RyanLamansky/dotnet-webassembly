using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Int32x4NotEqual instruction.</summary>
public class Int32x4NotEqual : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int32x4NotEqual"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int32x4NotEqual;

    /// <summary>Creates a new <see cref="Int32x4NotEqual"/> instance.</summary>
    public Int32x4NotEqual() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b) => (~Vector128.Equals(a.AsInt32(), b.AsInt32())).AsByte();
}
