using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Int16x8NotEqual instruction.</summary>
public class Int16x8NotEqual : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8NotEqual"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8NotEqual;

    /// <summary>Creates a new <see cref="Int16x8NotEqual"/> instance.</summary>
    public Int16x8NotEqual() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b) => (~Vector128.Equals(a.AsInt16(), b.AsInt16())).AsByte();
}
