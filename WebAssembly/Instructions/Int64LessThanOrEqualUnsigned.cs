namespace WebAssembly.Instructions;

/// <summary>
/// Unsigned less than or equal.
/// </summary>
public class Int64LessThanOrEqualUnsigned : ValueTwoToInt32NotEqualZeroInstruction
{
    /// <summary>
    /// Always <see cref="WebAssembly.OpCode.Int64LessThanOrEqualUnsigned"/>.
    /// </summary>
    public sealed override OpCode OpCode => OpCode.Int64LessThanOrEqualUnsigned;

    private protected sealed override WebAssemblyValueType ValueType => WebAssemblyValueType.Int64;

    private protected sealed override System.Reflection.Emit.OpCode EmittedOpCode =>
        System.Reflection.Emit.OpCodes.Cgt_Un; //The result is compared for equality to zero, reversing it.

    /// <summary>
    /// Creates a new  <see cref="Int64LessThanOrEqualUnsigned"/> instance.
    /// </summary>
    public Int64LessThanOrEqualUnsigned()
    {
    }
}
