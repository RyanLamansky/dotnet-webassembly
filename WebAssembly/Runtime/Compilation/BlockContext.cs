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
    /// True once reachable code has branched to this block's end label, meaning the end label is a real merge
    /// point that must be reached with a clean stack (results ferried through <see cref="ResultLocal"/>/<see cref="ResultLocals"/>).
    /// </summary>
    public bool IsEndLabelTargeted { get; private set; }

    /// <summary>
    /// For single-result blocks, the local that ferries the result value from each exit path (fall-through and
    /// branches) to the block's end label, keeping the IL evaluation stack balanced at the merge.
    /// </summary>
    public LocalBuilder? ResultLocal;

    /// <summary>
    /// For <c>if</c> blocks, the label that the <c>brfalse</c> emitted by <see cref="Instructions.If"/> jumps to:
    /// the <c>else</c> entry, or — when there is no <c>else</c> — the construct's exit.
    /// </summary>
    public Label? IfFalseLabel;

    /// <summary>
    /// For multi-value (type-index) blocks, the block's full signature (parameters → results); otherwise null.
    /// </summary>
    public Signature? BlockSignature;

    /// <summary>
    /// For multi-result blocks, the locals (one per result, first result at index 0) that ferry result values
    /// from each exit path to the block's end label.
    /// </summary>
    public LocalBuilder[]? ResultLocals;

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

    public void MarkEndLabelTargeted()
    {
        IsEndLabelTargeted = true;
    }
}
