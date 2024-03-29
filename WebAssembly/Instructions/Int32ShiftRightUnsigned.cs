namespace WebAssembly.Instructions;

/// <summary>
/// Sign-replicating (arithmetic) shift right.
/// </summary>
public class Int32ShiftRightUnsigned : ValueTwoToOneInstruction
{
    /// <summary>
    /// Always <see cref="OpCode.Int32ShiftRightUnsigned"/>.
    /// </summary>
    public sealed override OpCode OpCode => OpCode.Int32ShiftRightUnsigned;

    private protected sealed override WebAssemblyValueType ValueType => WebAssemblyValueType.Int32;

    private protected sealed override System.Reflection.Emit.OpCode EmittedOpCode =>
        System.Reflection.Emit.OpCodes.Shr_Un;

    /// <summary>
    /// Creates a new  <see cref="Int32ShiftRightUnsigned"/> instance.
    /// </summary>
    public Int32ShiftRightUnsigned()
    {
    }
}
