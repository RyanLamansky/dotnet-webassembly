using System.Linq;
using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Conditionally branch to a given label in an enclosing construct.
/// </summary>
public class BranchIf : Instruction
{
    /// <summary>
    /// Always <see cref="OpCode.BranchIf"/>.
    /// </summary>
    public sealed override OpCode OpCode => OpCode.BranchIf;

    /// <summary>
    /// The number of ancestor blocks to climb; 0 is the immediate parent.
    /// </summary>
    public uint Index { get; set; }

    /// <summary>
    /// Creates a new  <see cref="BranchIf"/> instance.
    /// </summary>
    public BranchIf()
    {
    }

    /// <summary>
    /// Creates a new <see cref="BranchIf"/> instance with the provided index.
    /// </summary>
    /// <param name="index">The number of ancestor blocks to climb; 0 is the immediate parent.</param>
    public BranchIf(uint index)
    {
        this.Index = index;
    }

    internal BranchIf(Reader reader)
    {
        Index = reader.ReadVarUInt32();
    }

    internal sealed override void WriteTo(Writer writer)
    {
        writer.Write((byte)OpCode.BranchIf);
        writer.WriteVar(this.Index);
    }

    /// <summary>
    /// Determines whether this instruction is identical to another.
    /// </summary>
    /// <param name="other">The instruction to compare against.</param>
    /// <returns>True if they have the same type and value, otherwise false.</returns>
    public override bool Equals(Instruction? other) =>
        other is BranchIf instruction
        && instruction.Index == this.Index
        ;

    /// <summary>
    /// Returns a simple hash code based on the value of the instruction.
    /// </summary>
    /// <returns>The hash code.</returns>
    public override int GetHashCode() => HashCode.Combine((int)this.OpCode, (int)this.Index);

    internal sealed override void Compile(CompilationContext context)
    {
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int32);
        var isReachable = !context.IsUnreachable;

        var blockType = context.Depth.ElementAt(checked((int)this.Index));
        var targetDepthKey = context.Depth.Count - checked((int)this.Index);
        var targetBlockContext = context.BlockContexts[targetDepthKey];
        var label = context.Labels[checked((uint)context.Depth.Count) - this.Index - 1];
        var isLoop = blockType.OpCode == OpCode.Loop;
        var branchTypes = BranchHelper.BranchTypes(context, targetDepthKey, targetBlockContext, isLoop);

        if (isReachable)
        {
            if (branchTypes.Length > 0)
            {
                BranchHelper.ValidateBranchTypes(context, this.OpCode, branchTypes, targetBlockContext);

                var discardCount = context.Stack.Count - targetBlockContext.InitialStackSize - branchTypes.Length;

                if (isLoop && discardCount == 0)
                {
                    // Loop back-edge with the parameters already in place: branch directly on the condition.
                    context.Emit(OpCodes.Brtrue, label);
                }
                else
                {
                    // Stack: [..baseline, intermediates, branchValues, condition]. Save the condition, stash the
                    // carried values, then on the taken path discard intermediates and (for a loop) reload the
                    // values; on the not-taken path restore the values above the intermediates.
                    var conditionLocal = context.DeclareLocal(typeof(int));
                    context.Emit(OpCodes.Stloc, conditionLocal);

                    LocalBuilder[] carriedLocals;
                    if (!isLoop)
                        carriedLocals = targetBlockContext.ResultLocals ??= context.DeclareResultLocals(branchTypes);
                    else
                        carriedLocals = context.DeclareResultLocals(branchTypes);

                    BranchHelper.StashResults(context, carriedLocals);

                    var skipTaken = context.DefineLabel();
                    context.Emit(OpCodes.Ldloc, conditionLocal);
                    context.Emit(OpCodes.Brfalse, skipTaken);

                    BranchHelper.DiscardIntermediates(context, discardCount);
                    if (isLoop)
                        BranchHelper.ReloadResults(context, carriedLocals);
                    context.Emit(OpCodes.Br, label);

                    context.MarkLabel(skipTaken);
                    BranchHelper.ReloadResults(context, carriedLocals);
                }
            }
            else if (!isLoop && blockType.Type.TryToValueType(out var expectedType))
            {
                context.ValidateStack(this.OpCode, expectedType);

                // Stack: [..baseline, intermediates, value, condition]. Duplicate the value into the result local
                // (so the taken path's reload at the end label has it), then either take the branch (discarding the
                // value and intermediates) or fall through with the value intact.
                var resultLocal = context.GetOrCreateResultLocal(checked((int)this.Index), expectedType);
                var conditionLocal = context.DeclareLocal(typeof(int));
                var skipTaken = context.DefineLabel();
                var intermediateCount = context.Stack.Count - targetBlockContext.InitialStackSize - 1;
                context.Emit(OpCodes.Stloc, conditionLocal);
                context.Emit(OpCodes.Dup);
                context.Emit(OpCodes.Stloc, resultLocal);
                context.Emit(OpCodes.Ldloc, conditionLocal);
                context.Emit(OpCodes.Brfalse, skipTaken);
                context.Emit(OpCodes.Pop); // Discard the duplicate left when taking the branch.
                BranchHelper.DiscardIntermediates(context, intermediateCount);
                context.Emit(OpCodes.Br, label);
                context.MarkLabel(skipTaken);
            }
            else
            {
                // Void target: on the taken path discard everything pushed inside the block before branching.
                var discardCount = context.Stack.Count - targetBlockContext.InitialStackSize;
                if (discardCount > 0)
                {
                    var skipTaken = context.DefineLabel();
                    var conditionLocal = context.DeclareLocal(typeof(int));
                    context.Emit(OpCodes.Stloc, conditionLocal);
                    context.Emit(OpCodes.Ldloc, conditionLocal);
                    context.Emit(OpCodes.Brfalse, skipTaken);
                    BranchHelper.DiscardIntermediates(context, discardCount);
                    context.Emit(OpCodes.Br, label);
                    context.MarkLabel(skipTaken);
                }
                else
                {
                    context.Emit(OpCodes.Brtrue, label);
                }
            }
        }
        else
        {
            BranchHelper.ValidateUnreachable(context, this.OpCode, blockType, branchTypes, isLoop, targetBlockContext);
            context.Emit(OpCodes.Brtrue, label);
        }

        // Code after a conditional branch is reachable only when the branch itself was reachable.
        if (isReachable)
            context.MarkReachable();
    }

    /// <summary>
    /// Provides a native representation of the instruction and the block index
    /// </summary>
    /// <returns>A string representation of this instance and the block index.</returns>
    public override string ToString() => $"{base.ToString()} {Index}";
}
