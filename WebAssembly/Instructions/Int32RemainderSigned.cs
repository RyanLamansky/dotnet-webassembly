namespace WebAssembly.Instructions;

/// <summary>
/// Signed remainder (result has the sign of the dividend).
/// </summary>
public class Int32RemainderSigned : ValueTwoToOneInstruction
{
    /// <summary>
    /// Always <see cref="OpCode.Int32RemainderSigned"/>.
    /// </summary>
    public sealed override OpCode OpCode => OpCode.Int32RemainderSigned;

    private protected sealed override WebAssemblyValueType ValueType => WebAssemblyValueType.Int32;

    private protected sealed override System.Reflection.Emit.OpCode EmittedOpCode =>
        System.Reflection.Emit.OpCodes.Rem;

    /// <summary>
    /// Creates a new  <see cref="Int32RemainderSigned"/> instance.
    /// </summary>
    public Int32RemainderSigned()
    {
    }
}
