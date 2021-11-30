namespace WebAssembly.Instructions;

/// <summary>
/// Subtraction.
/// </summary>
public class Float64Subtract : ValueTwoToOneInstruction
{
    /// <summary>
    /// Always <see cref="OpCode.Float64Subtract"/>.
    /// </summary>
    public sealed override OpCode OpCode => OpCode.Float64Subtract;

    private protected sealed override WebAssemblyValueType ValueType => WebAssemblyValueType.Float64;

    private protected sealed override System.Reflection.Emit.OpCode EmittedOpCode =>
        System.Reflection.Emit.OpCodes.Sub;

    /// <summary>
    /// Creates a new  <see cref="Float64Subtract"/> instance.
    /// </summary>
    public Float64Subtract()
    {
    }
}
