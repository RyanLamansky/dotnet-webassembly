using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Test whether any lane is non-zero.</summary>
public class V128AnyTrue : SimdV128ToI32Instruction
{
    /// <summary>Always <see cref="SimdOpCode.V128AnyTrue"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.V128AnyTrue;

    /// <summary>Creates a new <see cref="V128AnyTrue"/> instance.</summary>
    public V128AnyTrue() { }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static int Execute(Vector128<byte> a) => Vector128.EqualsAll(a, Vector128<byte>.Zero) ? 0 : 1;
}
