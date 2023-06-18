using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Initializes memory bytes.
/// </summary>
public class MemoryInitialize : MiscellaneousInstruction
{
    /// <summary>
    /// Always <see cref="MiscellaneousOpCode.MemoryInitialize"/>.
    /// </summary>
    public sealed override MiscellaneousOpCode MiscellaneousOpCode => MiscellaneousOpCode.MemoryInitialize;

    internal sealed override void Compile(CompilationContext context)
    {
        throw new System.NotImplementedException();
    }
}
