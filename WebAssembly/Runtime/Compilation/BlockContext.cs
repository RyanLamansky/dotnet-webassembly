using System.Reflection.Emit;

namespace WebAssembly.Runtime.Compilation;

/// <summary>
/// Remembers stack state at the beginning of a block, and reachability of code.
/// </summary>
internal sealed class BlockContext
{
    public readonly int InitialStackSize;
    public bool IsUnreachable { get; private set; }

    /// <summary>
    /// For typed blocks (non-void), holds the local that ferries the result value across branches.
    /// </summary>
    public LocalBuilder? ResultLocal;

    /// <summary>
    /// For <c>if</c> blocks, holds the false-branch label emitted by <c>brfalse</c>.
    /// <c>else</c> marks this label; <c>end</c> with no else also marks it.
    /// </summary>
    public Label? IfFalseLabel;

    public BlockContext()
    {
    }

    public BlockContext(int initialStackSize)
    {
        InitialStackSize = initialStackSize;
    }

    public void MarkUnreachable()
    {
        IsUnreachable = true;
    }

    public void MarkReachable()
    {
        IsUnreachable = false;
    }
}
