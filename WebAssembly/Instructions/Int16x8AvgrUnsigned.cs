using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Int16x8AvgrUnsigned instruction.</summary>
public class Int16x8AvgrUnsigned : SimdBinaryV128Instruction
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8AvgrUnsigned"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8AvgrUnsigned;

    /// <summary>Creates a new <see cref="Int16x8AvgrUnsigned"/> instance.</summary>
    public Int16x8AvgrUnsigned() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(Vector128<byte> a, Vector128<byte> b) { var r = new ushort[8]; for (var i = 0; i < 8; i++) r[i] = (ushort)((a.AsUInt16().GetElement(i) + b.AsUInt16().GetElement(i) + 1) >> 1); return Vector128.Create(r).AsByte(); }
}
