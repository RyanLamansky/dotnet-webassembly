namespace WebAssembly.Instructions;

/// <summary>
/// Sign-agnostic bitwise inclusive or.
/// </summary>
public class Int32Or : ValueTwoToOneInstruction
{
    /// <summary>
    /// Always <see cref="OpCode.Int32Or"/>.
    /// </summary>
    public sealed override OpCode OpCode => OpCode.Int32Or;

    private protected sealed override WebAssemblyValueType ValueType => WebAssemblyValueType.Int32;

    private protected sealed override System.Reflection.Emit.OpCode EmittedOpCode =>
        System.Reflection.Emit.OpCodes.Or;

    /// <summary>
    /// Creates a new  <see cref="Int32Or"/> instance.
    /// </summary>
    public Int32Or()
    {
    }
}
