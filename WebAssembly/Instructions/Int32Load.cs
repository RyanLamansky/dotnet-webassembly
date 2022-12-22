using System.Reflection.Emit;

namespace WebAssembly.Instructions;

/// <summary>
/// Load 4 bytes as i32.
/// </summary>
public class Int32Load : MemoryReadInstruction
{
    /// <summary>
    /// Always <see cref="WebAssembly.OpCode.Int32Load"/>.
    /// </summary>
    public sealed override OpCode OpCode => OpCode.Int32Load;

    /// <summary>
    /// Creates a new  <see cref="Int32Load"/> instance.
    /// </summary>
    public Int32Load()
    {
    }

    internal Int32Load(Reader reader)
        : base(reader)
    {
    }

    private protected sealed override WebAssemblyValueType Type => WebAssemblyValueType.Int32;

    private protected sealed override byte Size => 4;

    private protected sealed override System.Reflection.Emit.OpCode EmittedOpCode => OpCodes.Ldind_I4;
}
