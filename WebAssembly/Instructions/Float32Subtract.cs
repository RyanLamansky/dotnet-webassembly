namespace WebAssembly.Instructions;

/// <summary>
/// Subtraction.
/// </summary>
public class Float32Subtract : ValueTwoToOneInstruction
{
    /// <summary>
    /// Always <see cref="OpCode.Float32Subtract"/>.
    /// </summary>
    public sealed override OpCode OpCode => OpCode.Float32Subtract;

    private protected sealed override WebAssemblyValueType ValueType => WebAssemblyValueType.Float32;

    private protected sealed override System.Reflection.Emit.OpCode EmittedOpCode =>
        System.Reflection.Emit.OpCodes.Sub;

    /// <summary>
    /// Creates a new  <see cref="Float32Subtract"/> instance.
    /// </summary>
    public Float32Subtract()
    {
    }
}
