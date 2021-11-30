namespace WebAssembly.Instructions;

/// <summary>
/// Sign-agnostic bitwise and.
/// </summary>
public class Int64And : ValueTwoToOneInstruction
{
    /// <summary>
    /// Always <see cref="OpCode.Int64And"/>.
    /// </summary>
    public sealed override OpCode OpCode => OpCode.Int64And;

    private protected sealed override WebAssemblyValueType ValueType => WebAssemblyValueType.Int64;

    private protected sealed override System.Reflection.Emit.OpCode EmittedOpCode =>
        System.Reflection.Emit.OpCodes.And;

    /// <summary>
    /// Creates a new  <see cref="Int64And"/> instance.
    /// </summary>
    public Int64And()
    {
    }
}
