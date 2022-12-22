namespace WebAssembly.Instructions;

/// <summary>
/// Unsigned greater than.
/// </summary>
public class Int32GreaterThanUnsigned : ValueTwoToOneInstruction
{
    /// <summary>
    /// Always <see cref="WebAssembly.OpCode.Int32GreaterThanUnsigned"/>.
    /// </summary>
    public sealed override OpCode OpCode => OpCode.Int32GreaterThanUnsigned;

    private protected sealed override WebAssemblyValueType ValueType => WebAssemblyValueType.Int32;

    private protected sealed override System.Reflection.Emit.OpCode EmittedOpCode =>
        System.Reflection.Emit.OpCodes.Cgt_Un;

    /// <summary>
    /// Creates a new  <see cref="Int32GreaterThanUnsigned"/> instance.
    /// </summary>
    public Int32GreaterThanUnsigned()
    {
    }
}
