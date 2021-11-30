namespace WebAssembly.Instructions;

/// <summary>
/// Multiplication.
/// </summary>
public class Float64Multiply : ValueTwoToOneInstruction
{
    /// <summary>
    /// Always <see cref="OpCode.Float64Multiply"/>.
    /// </summary>
    public sealed override OpCode OpCode => OpCode.Float64Multiply;

    private protected sealed override WebAssemblyValueType ValueType => WebAssemblyValueType.Float64;

    private protected sealed override System.Reflection.Emit.OpCode EmittedOpCode =>
        System.Reflection.Emit.OpCodes.Mul;

    /// <summary>
    /// Creates a new  <see cref="Float64Multiply"/> instance.
    /// </summary>
    public Float64Multiply()
    {
    }
}
