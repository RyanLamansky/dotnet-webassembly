namespace WebAssembly.Instructions;

/// <summary>
/// Signed greater than.
/// </summary>
public class Int32GreaterThanSigned : ValueTwoToOneInstruction
{
    /// <summary>
    /// Always <see cref="WebAssembly.OpCode.Int32GreaterThanSigned"/>.
    /// </summary>
    public sealed override OpCode OpCode => OpCode.Int32GreaterThanSigned;

    private protected sealed override WebAssemblyValueType ValueType => WebAssemblyValueType.Int32;

    private protected sealed override System.Reflection.Emit.OpCode EmittedOpCode =>
        System.Reflection.Emit.OpCodes.Cgt;

    /// <summary>
    /// Creates a new  <see cref="Int32GreaterThanSigned"/> instance.
    /// </summary>
    public Int32GreaterThanSigned()
    {
    }
}
