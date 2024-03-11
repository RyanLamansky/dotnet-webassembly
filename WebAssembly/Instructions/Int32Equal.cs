namespace WebAssembly.Instructions;

/// <summary>
/// Sign-agnostic compare equal.
/// </summary>
public class Int32Equal : ValueTwoToOneInstruction
{
    /// <summary>
    /// Always <see cref="WebAssembly.OpCode.Int32Equal"/>.
    /// </summary>
    public sealed override OpCode OpCode => OpCode.Int32Equal;

    private protected sealed override WebAssemblyValueType ValueType => WebAssemblyValueType.Int32;

    private protected sealed override System.Reflection.Emit.OpCode EmittedOpCode =>
        System.Reflection.Emit.OpCodes.Ceq;

    /// <summary>
    /// Creates a new  <see cref="Int32Equal"/> instance.
    /// </summary>
    public Int32Equal()
    {
    }
}
