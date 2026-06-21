using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Splat an i32 to all i16x8 lanes.</summary>
public class Int16x8Splat : SimdSplatInstruction
{
    /// <summary>Always <see cref="SimdOpCode.Int16x8Splat"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16x8Splat;
    internal override WebAssemblyValueType ScalarType => WebAssemblyValueType.Int32;

    /// <summary>Creates a new <see cref="Int16x8Splat"/> instance.</summary>
    public Int16x8Splat() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(int x) => Vector128.Create((short)(x & 0xFFFF)).AsByte();
}
