using System.Collections.Generic;
using System.Reflection.Emit;
using WebAssembly.Instructions;

namespace WebAssembly.Runtime.Compilation;

/// <summary>
/// State for one live <see cref="TryTable"/> protected region (WASM 3.0 exception handling). ECMA-335 requires the
/// IL evaluation stack to be empty at try entry and cleared by <c>leave</c>, so on entry the region spills the
/// physically-present operand stack into <see cref="SpillLocals"/> and reloads only the block's parameters inside the
/// try; branches that leave the region reconstruct the physical stack from the spill in an <see cref="ExitStub"/>.
/// </summary>
internal sealed class ProtectedRegion
{
    /// <summary>The absolute stack index of the first slot this region spilled — the physical floor of the enclosing region (or 0).</summary>
    public readonly int SpillStart;

    /// <summary>The spilled segment <c>[SpillStart, entry stack height)</c>, one local per slot in bottom-to-top order.</summary>
    public readonly LocalBuilder[] SpillLocals;

    /// <summary>The physical floor inside the region: the try block's <see cref="BlockContext.InitialStackSize"/>.</summary>
    public readonly int Floor;

    /// <summary>The absolute block-depth key (<c>Depth.Count</c>) of the try block whose region this is.</summary>
    public readonly int DepthKey;

    /// <summary>The exception block's leave target (from <c>BeginExceptionBlock</c>); the fall-through resume point.</summary>
    public readonly Label EhEndLabel;

    /// <summary>The catch clauses installed by the try block, tried in order.</summary>
    public readonly IList<CatchClause> Clauses;

    /// <summary>Exit stubs for branches leaving this region, keyed by absolute target block-depth key.</summary>
    public readonly Dictionary<int, ExitStub> ExitStubs = [];

    public ProtectedRegion(int spillStart, LocalBuilder[] spillLocals, int floor, int depthKey, Label ehEndLabel, IList<CatchClause> clauses)
    {
        this.SpillStart = spillStart;
        this.SpillLocals = spillLocals;
        this.Floor = floor;
        this.DepthKey = depthKey;
        this.EhEndLabel = ehEndLabel;
        this.Clauses = clauses;
    }

    /// <summary>
    /// A per-target trampoline that a crossing <c>leave</c> lands on: it reconstructs the physical stack slice
    /// <c>[SpillStart, TargetBaseline)</c> from <see cref="SpillLocals"/> (plus, for a loop target, the carried
    /// values from <see cref="CarriedLocals"/>) and then <c>br</c>s to the real target label.
    /// </summary>
    internal sealed class ExitStub
    {
        public readonly Label StubLabel;
        public readonly int TargetDepthKey;
        public readonly Label TargetLabel;
        public readonly int TargetBaseline;
        public readonly bool IsLoop;
        public readonly LocalBuilder[]? CarriedLocals;

        public ExitStub(Label stubLabel, int targetDepthKey, Label targetLabel, int targetBaseline, bool isLoop, LocalBuilder[]? carriedLocals)
        {
            this.StubLabel = stubLabel;
            this.TargetDepthKey = targetDepthKey;
            this.TargetLabel = targetLabel;
            this.TargetBaseline = targetBaseline;
            this.IsLoop = isLoop;
            this.CarriedLocals = carriedLocals;
        }
    }
}
