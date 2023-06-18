using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Fills memory bytes to a specific value.
/// </summary>
public class MemoryFill : MiscellaneousInstruction
{
    /// <summary>
    /// Always <see cref="MiscellaneousOpCode.MemoryFill"/>.
    /// </summary>
    public sealed override MiscellaneousOpCode MiscellaneousOpCode => MiscellaneousOpCode.MemoryFill;

    internal sealed override void Compile(CompilationContext context)
    {
        throw new System.NotImplementedException();
    }
}
