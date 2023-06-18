using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Copy bytes from one area in memory to another.
/// </summary>
public class MemoryCopy : MiscellaneousInstruction
{
    /// <summary>
    /// Always <see cref="MiscellaneousOpCode.MemoryCopy"/>.
    /// </summary>
    public sealed override MiscellaneousOpCode MiscellaneousOpCode => MiscellaneousOpCode.MemoryCopy;

    internal sealed override void Compile(CompilationContext context)
    {
        throw new System.NotImplementedException();
    }
}
