using System.Reflection.Emit;

namespace WebAssembly.Instructions;

/// <summary>
/// Load 1 byte and sign-extend i8 to i32.
/// </summary>
public class Int32Load8Signed : MemoryReadInstruction
{
    /// <summary>
    /// Always <see cref="OpCode.Int32Load8Signed"/>.
    /// </summary>
    public sealed override OpCode OpCode => OpCode.Int32Load8Signed;

    /// <summary>
    /// Creates a new  <see cref="Int32Load8Signed"/> instance.
    /// </summary>
    public Int32Load8Signed()
    {
    }

    internal Int32Load8Signed(Reader reader)
        : base(reader)
    {
    }

    private protected sealed override WebAssemblyValueType Type => WebAssemblyValueType.Int32;

    private protected sealed override byte Size => 1;

    private protected sealed override System.Reflection.Emit.OpCode EmittedOpCode => OpCodes.Ldind_I1;
}
