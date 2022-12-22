using System.Reflection.Emit;

namespace WebAssembly.Instructions;

/// <summary>
/// Negation.
/// </summary>
public class Float64Negate : ValueOneToOneInstruction
{
    /// <summary>
    /// Always <see cref="WebAssembly.OpCode.Float64Negate"/>.
    /// </summary>
    public sealed override OpCode OpCode => OpCode.Float64Negate;

    /// <summary>
    /// Creates a new  <see cref="Float64Negate"/> instance.
    /// </summary>
    public Float64Negate()
    {
    }

    private protected sealed override System.Reflection.Emit.OpCode EmittedOpCode => OpCodes.Neg;

    private protected sealed override WebAssemblyValueType ValueType => WebAssemblyValueType.Float64;
}
