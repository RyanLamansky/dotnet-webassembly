namespace WebAssembly.Instructions;

/// <summary>
/// Sign-agnostic addition.
/// </summary>
public class Int64Add : ValueTwoToOneInstruction
{
    /// <summary>
    /// Always <see cref="WebAssembly.OpCode.Int64Add"/>.
    /// </summary>
    public sealed override OpCode OpCode => OpCode.Int64Add;

    private protected sealed override WebAssemblyValueType ValueType => WebAssemblyValueType.Int64;

    private protected sealed override System.Reflection.Emit.OpCode EmittedOpCode =>
        System.Reflection.Emit.OpCodes.Add;

    /// <summary>
    /// Creates a new  <see cref="Int64Add"/> instance.
    /// </summary>
    public Int64Add()
    {
    }
}
