using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Wrap i64 to i16 and store 2 bytes.
/// </summary>
public class Int64Store16 : MemoryWriteInstruction
{
    /// <summary>
    /// Always <see cref="WebAssembly.OpCode.Int64Store16"/>.
    /// </summary>
    public sealed override OpCode OpCode => OpCode.Int64Store16;

    /// <summary>
    /// Creates a new  <see cref="Int64Store16"/> instance.
    /// </summary>
    public Int64Store16()
    {
    }

    internal Int64Store16(Reader reader)
        : base(reader)
    {
    }

    private protected sealed override WebAssemblyValueType Type => WebAssemblyValueType.Int64;

    private protected sealed override byte Size => 2;

    private protected sealed override System.Reflection.Emit.OpCode EmittedOpCode => OpCodes.Stind_I2;

    private protected sealed override HelperMethod StoreHelper => HelperMethod.StoreInt16FromInt64;
}
