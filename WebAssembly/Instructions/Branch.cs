using System;
using System.Linq;
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
        var targetBlockContext = context.BlockContexts[targetDepthKey];
        var label = context.Labels[checked((uint)context.Depth.Count) - this.Index - 1];

        var isLoop = blockType.OpCode == OpCode.Loop;
        var branchTypes = BranchHelper.BranchTypes(context, targetDepthKey, targetBlockContext, isLoop);

        if (!context.IsUnreachable)
        {
            if (branchTypes.Length > 0)
            {
                BranchHelper.ValidateBranchTypes(context, this.OpCode, branchTypes, targetBlockContext);

                var discardCount = context.Stack.Count - targetBlockContext.InitialStackSize - branchTypes.Length;

                if (!isLoop)
                {
                    // Stash the results into the target's result locals (reloaded at its end label), then discard any
                    // intermediates left beneath them.
                    targetBlockContext.ResultLocals ??= context.DeclareResultLocals(branchTypes);
                    for (var i = branchTypes.Length - 1; i >= 0; i--)
                        context.Emit(OpCodes.Stloc, targetBlockContext.ResultLocals[i]);
                    for (var i = 0; i < discardCount; i++)
                        context.Emit(OpCodes.Pop);
                }
                else if (discardCount > 0)
                {
                    // Loop back-edge with intermediates beneath the parameters: temporarily stash the parameters so
                    // the intermediates can be discarded, then restore the parameters as the back-edge values.
                    var temporaries = context.DeclareResultLocals(branchTypes);
                    for (var i = branchTypes.Length - 1; i >= 0; i--)
                        context.Emit(OpCodes.Stloc, temporaries[i]);
                    for (var i = 0; i < discardCount; i++)
                        context.Emit(OpCodes.Pop);
                    for (var i = 0; i < branchTypes.Length; i++)
                        context.Emit(OpCodes.Ldloc, temporaries[i]);
                }
                // else: loop back-edge with the parameters already in place — branch directly.
            }
            else if (!isLoop && blockType.Type.TryToValueType(out var expectedType))
            {
                context.ValidateStack(this.OpCode, expectedType);
                context.Emit(OpCodes.Stloc, context.GetOrCreateResultLocal(checked((int)this.Index), expectedType));

                var intermediateCount = context.Stack.Count - targetBlockContext.InitialStackSize - 1;
                for (var i = 0; i < intermediateCount; i++)
                    context.Emit(OpCodes.Pop);
            }
            else
            {
                // Void target: discard every value pushed inside this block before branching.
                var discardCount = context.Stack.Count - targetBlockContext.InitialStackSize;
                for (var i = 0; i < discardCount; i++)
                    context.Emit(OpCodes.Pop);
            }
        }
        else
        {
            BranchHelper.ValidateUnreachable(context, this.OpCode, blockType, branchTypes, isLoop, targetBlockContext);
        }

        context.Emit(OpCodes.Br, label);

        //Mark the subsequent code within this block is unreachable
        context.MarkUnreachable();
    }
}
