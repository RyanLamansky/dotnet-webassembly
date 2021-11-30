using System.Reflection.Emit;

namespace WebAssembly.Instructions;

/// <summary>
/// Negation.
/// </summary>
public class Float32Negate : ValueOneToOneInstruction
{
    /// <summary>
    /// Always <see cref="OpCode.Float32Negate"/>.
    /// </summary>
    public sealed override OpCode OpCode => OpCode.Float32Negate;

    /// <summary>
    /// Creates a new  <see cref="Float32Negate"/> instance.
    /// </summary>
    public Float32Negate()
    {
    }

    private protected sealed override System.Reflection.Emit.OpCode EmittedOpCode => OpCodes.Neg;

    private protected sealed override WebAssemblyValueType ValueType => WebAssemblyValueType.Float32;
}
