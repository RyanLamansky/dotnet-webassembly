namespace WebAssembly.Instructions;

/// <summary>
/// Multiplication.
/// </summary>
public class Float32Multiply : ValueTwoToOneInstruction
{
    /// <summary>
    /// Always <see cref="WebAssembly.OpCode.Float32Multiply"/>.
    /// </summary>
    public sealed override OpCode OpCode => OpCode.Float32Multiply;

    private protected sealed override WebAssemblyValueType ValueType => WebAssemblyValueType.Float32;

    private protected sealed override System.Reflection.Emit.OpCode EmittedOpCode =>
        System.Reflection.Emit.OpCodes.Mul;

    /// <summary>
    /// Creates a new  <see cref="Float32Multiply"/> instance.
    /// </summary>
    public Float32Multiply()
    {
    }
}
