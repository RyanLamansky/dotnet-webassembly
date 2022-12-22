namespace WebAssembly.Instructions;

/// <summary>
/// Unsigned division (result is floored).
/// </summary>
public class Int64DivideUnsigned : ValueTwoToOneInstruction
{
    /// <summary>
    /// Always <see cref="WebAssembly.OpCode.Int64DivideUnsigned"/>.
    /// </summary>
    public sealed override OpCode OpCode => OpCode.Int64DivideUnsigned;

    private protected sealed override WebAssemblyValueType ValueType => WebAssemblyValueType.Int64;

    private protected sealed override System.Reflection.Emit.OpCode EmittedOpCode =>
        System.Reflection.Emit.OpCodes.Div_Un;

    /// <summary>
    /// Creates a new  <see cref="Int64DivideUnsigned"/> instance.
    /// </summary>
    public Int64DivideUnsigned()
    {
    }
}
