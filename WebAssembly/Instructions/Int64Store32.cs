using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Wrap i64 to i32 and store 4 bytes.
/// </summary>
public class Int64Store32 : MemoryWriteInstruction
{
    /// <summary>
    /// Always <see cref="OpCode.Int64Store32"/>.
    /// </summary>
    public sealed override OpCode OpCode => OpCode.Int64Store32;

    /// <summary>
    /// Creates a new  <see cref="Int64Store32"/> instance.
    /// </summary>
    public Int64Store32()
    {
    }

    internal Int64Store32(Reader reader)
        : base(reader)
    {
    }

    private protected sealed override WebAssemblyValueType Type => WebAssemblyValueType.Int64;

    private protected sealed override byte Size => 4;

    private protected sealed override System.Reflection.Emit.OpCode EmittedOpCode => OpCodes.Stind_I4;

    private protected sealed override HelperMethod StoreHelper => HelperMethod.StoreInt32FromInt64;
}
