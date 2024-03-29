namespace WebAssembly.Instructions;

/// <summary>
/// Sign-agnostic shift left.
/// </summary>
public class Int32ShiftLeft : ValueTwoToOneInstruction
{
    /// <summary>
    /// Always <see cref="OpCode.Int32ShiftLeft"/>.
    /// </summary>
    public sealed override OpCode OpCode => OpCode.Int32ShiftLeft;

    private protected sealed override WebAssemblyValueType ValueType => WebAssemblyValueType.Int32;

    private protected sealed override System.Reflection.Emit.OpCode EmittedOpCode =>
        System.Reflection.Emit.OpCodes.Shl;

    /// <summary>
    /// Creates a new  <see cref="Int32ShiftLeft"/> instance.
    /// </summary>
    public Int32ShiftLeft()
    {
    }
}
