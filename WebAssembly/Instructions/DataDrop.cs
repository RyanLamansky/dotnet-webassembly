using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Drop a data element to release memory.
/// </summary>
public class DataDrop : MiscellaneousInstruction
{
    /// <summary>
    /// Always <see cref="MiscellaneousOpCode.DataDrop"/>.
    /// </summary>
    public sealed override MiscellaneousOpCode MiscellaneousOpCode => MiscellaneousOpCode.DataDrop;

    internal sealed override void Compile(CompilationContext context)
    {
        throw new System.NotImplementedException();
    }
}
