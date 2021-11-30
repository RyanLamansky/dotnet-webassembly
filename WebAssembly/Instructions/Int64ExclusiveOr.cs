namespace WebAssembly.Instructions;

/// <summary>
/// Sign-agnostic bitwise exclusive or.
/// </summary>
public class Int64ExclusiveOr : ValueTwoToOneInstruction
{
    /// <summary>
    /// Always <see cref="OpCode.Int64ExclusiveOr"/>.
    /// </summary>
    public sealed override OpCode OpCode => OpCode.Int64ExclusiveOr;

    private protected sealed override WebAssemblyValueType ValueType => WebAssemblyValueType.Int64;

    private protected sealed override System.Reflection.Emit.OpCode EmittedOpCode =>
        System.Reflection.Emit.OpCodes.Xor;

    /// <summary>
    /// Creates a new  <see cref="Int64ExclusiveOr"/> instance.
    /// </summary>
    public Int64ExclusiveOr()
    {
    }
}
