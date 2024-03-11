namespace WebAssembly.Instructions;

/// <summary>
/// Sign-agnostic multiplication (lower 64-bits).
/// </summary>
public class Int64Multiply : ValueTwoToOneInstruction
{
    /// <summary>
    /// Always <see cref="WebAssembly.OpCode.Int64Multiply"/>.
    /// </summary>
    public sealed override OpCode OpCode => OpCode.Int64Multiply;

    private protected sealed override WebAssemblyValueType ValueType => WebAssemblyValueType.Int64;

    private protected sealed override System.Reflection.Emit.OpCode EmittedOpCode =>
        System.Reflection.Emit.OpCodes.Mul;

    /// <summary>
    /// Creates a new  <see cref="Int64Multiply"/> instance.
    /// </summary>
    public Int64Multiply()
    {
    }
}
