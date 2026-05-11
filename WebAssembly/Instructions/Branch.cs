using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Branch to a given label in an enclosing construct.
/// </summary>
public class Branch : Instruction
{
    /// <summary>
    /// Always <see cref="OpCode.Branch"/>.
    /// </summary>
    public sealed override OpCode OpCode => OpCode.Branch;

    /// <summary>
    /// The number of ancestor blocks to climb; 0 is the immediate parent.
    /// </summary>
    public uint Index { get; set; }

    /// <summary>
    /// Creates a new  <see cref="Branch"/> instance.
    /// </summary>
    public Branch()
    {
    }

    /// <summary>
    /// Creates a new <see cref="Branch"/> instance with the provided index.
    /// </summary>
    /// <param name="index">The number of ancestor blocks to climb; 0 is the immediate parent.</param>
    public Branch(uint index)
    {
        this.Index = index;
    }

    internal Branch(Reader reader)
    {
        Index = reader.ReadVarUInt32();
    }

    internal sealed override void WriteTo(Writer writer)
    {
        writer.Write((byte)OpCode.Branch);
        writer.WriteVar(this.Index);
    }

    /// <summary>
    /// Determines whether this instruction is identical to another.
    /// </summary>
    /// <param name="other">The instruction to compare against.</param>
    /// <returns>True if they have the same type and value, otherwise false.</returns>
    public override bool Equals(Instruction? other) =>
        other is Branch instruction
        && instruction.Index == this.Index
        ;

    /// <summary>
    /// Returns a simple hash code based on the value of the instruction.
    /// </summary>
    /// <returns>The hash code.</returns>
    public override int GetHashCode() => HashCode.Combine((int)this.OpCode, (int)this.Index);

    /// <summary>
    /// Provides a native representation of the instruction.
    /// </summary>
    /// <returns>A string representation of this instance.</returns>
    public override string ToString() => $"{base.ToString()} {Index}";

    internal sealed override void Compile(CompilationContext context)
    {
        var blockType = context.Depth.ElementAt(checked((int)this.Index));
        var targetDepthKey = context.Depth.Count - checked((int)this.Index);
        var targetBlockCtx = context.BlockContexts[targetDepthKey];

        // Determine the branch arity: for loops it's their params; for blocks/ifs it's their results.
        var isLoop = blockType.OpCode == OpCode.Loop;
        var branchSig = targetBlockCtx.BlockSignature;
        var functionReturns = !isLoop && targetDepthKey == 1 && branchSig == null
            ? context.CheckedSignature.RawReturnTypes
            : System.Array.Empty<WebAssemblyValueType>();
        var branchTypes = branchSig != null
            ? (isLoop ? branchSig.RawParameterTypes : branchSig.RawReturnTypes)
            : functionReturns.Length > 1 ? functionReturns : System.Array.Empty<WebAssemblyValueType>();

        if (!context.IsUnreachable)
        {
            if (!isLoop)
                targetBlockCtx.MarkEndLabelTargeted();

            if (branchTypes.Length > 1)
            {
                var available = context.Stack.Count - targetBlockCtx.InitialStackSize;
                if (available < branchTypes.Length)
                    throw new StackSizeIncorrectException(this.OpCode, branchTypes.Length, available);
                // Validate types (bottom-up: stackSnapshot[0] is TOS).
                var stackSnapshot = context.Stack.ToArray();
                for (var k = 0; k < branchTypes.Length; k++)
                {
                    var actual = stackSnapshot[branchTypes.Length - 1 - k];
                    if (actual != branchTypes[k])
                        throw new StackTypeInvalidException(this.OpCode, branchTypes[k], actual);
                }
                if (!isLoop && branchTypes.Length > 0)
                {
                    // Stash results into ResultLocals (create if needed).
                    if (targetBlockCtx.ResultLocals == null)
                    {
                        targetBlockCtx.ResultLocals = new LocalBuilder[branchTypes.Length];
                        for (var i = 0; i < branchTypes.Length; i++)
                            targetBlockCtx.ResultLocals[i] = context.DeclareLocal(branchTypes[i].ToSystemType());
                    }
                    for (var i = branchTypes.Length - 1; i >= 0; i--)
                        context.Emit(OpCodes.Stloc, targetBlockCtx.ResultLocals[i]);
                }
                // Pop all intermediates above the baseline (excluding the results already stloc'd for non-loop,
                // or the params that stay on the IL stack for a loop back-edge).
                var discardCount = context.Stack.Count - targetBlockCtx.InitialStackSize - branchTypes.Length;
                for (var i = 0; i < discardCount; i++)
                    context.Emit(OpCodes.Pop);
            }
            else if (!isLoop && blockType.Type.TryToValueType(out var expectedType))
            {
                context.ValidateStack(this.OpCode, expectedType);
                var resultLocal = context.GetOrCreateResultLocal(checked((int)this.Index), expectedType);
                // After ValidateStack: tracking stack = [..., intermediates..., T]
                // Stash T, pop intermediates, then branch.
                context.Emit(OpCodes.Stloc, resultLocal);
                var intermediateCount = context.Stack.Count - targetBlockCtx.InitialStackSize - 1;
                for (var i = 0; i < intermediateCount; i++)
                    context.Emit(OpCodes.Pop);
            }
            else
            {
                // True void block: discard all values pushed inside this block before branching.
                var discardCount = context.Stack.Count - targetBlockCtx.InitialStackSize;
                for (var i = 0; i < discardCount; i++)
                    context.Emit(OpCodes.Pop);
            }
        }
        else if (!isLoop && branchSig == null && blockType.Type.TryToValueType(out var expectedType2))
        {
            // In unreachable mode: still validate stack (for type checking), no IL emitted.
            context.ValidateStack(this.OpCode, expectedType2);
        }

        context.Emit(OpCodes.Br, context.Labels[checked((uint)context.Depth.Count) - this.Index - 1]);

        //Mark the subsequent code within this block is unreachable
        context.MarkUnreachable();
    }
}
