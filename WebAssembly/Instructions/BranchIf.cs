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

        var blockType = context.Depth.ElementAt(checked((int)this.Index));
        var label = context.Labels[checked((uint)context.Depth.Count) - this.Index - 1];

        if (blockType.OpCode != OpCode.Loop && blockType.Type.TryToValueType(out var expectedType))
        {
            context.ValidateStack(this.OpCode, expectedType);

            if (!context.IsUnreachable)
            {
                var targetBlockCtx = context.BlockContexts[context.Depth.Count - checked((int)this.Index)];
                var resultLocal = context.GetOrCreateResultLocal(checked((int)this.Index), expectedType);
                var condLocal = context.DeclareLocal(typeof(int));
                // IL stack: [..., intermediates..., value, cond]
                // Save cond, dup value into result local (for taken path), reload cond.
                // On taken: pop value + intermediates, then br label (empty IL stack at label).
                // On not-taken: leave [..., intermediates..., value] on stack.
                var skipTaken = context.DefineLabel();
                var intermediateCount = context.Stack.Count - targetBlockCtx.InitialStackSize - 1;
                context.Emit(OpCodes.Stloc, condLocal);
                context.Emit(OpCodes.Dup);
                context.Emit(OpCodes.Stloc, resultLocal);
                context.Emit(OpCodes.Ldloc, condLocal);
                context.Emit(OpCodes.Brfalse, skipTaken);
                context.Emit(OpCodes.Pop); // pop the duplicated value
                for (var i = 0; i < intermediateCount; i++)
                    context.Emit(OpCodes.Pop);
                context.Emit(OpCodes.Br, label);
                context.MarkLabel(skipTaken);
            }
            else
            {
                context.Emit(OpCodes.Brtrue, label);
            }
        }
        else if (!context.IsUnreachable)
        {
            // Void block: on taken path, discard all intermediate values before jumping.
            var targetBlockCtx = context.BlockContexts[context.Depth.Count - checked((int)this.Index)];
            var discardCount = context.Stack.Count - targetBlockCtx.InitialStackSize;
            if (discardCount > 0)
            {
                // Need to conditionally discard intermediates before jumping.
                var skipTaken = context.DefineLabel();
                var condLocal = context.DeclareLocal(typeof(int));
                context.Emit(OpCodes.Stloc, condLocal);         // save cond; stack = [..., intermediates...]
                context.Emit(OpCodes.Ldloc, condLocal);
                context.Emit(OpCodes.Brfalse, skipTaken);       // if false, skip the cleanup
                for (var i = 0; i < discardCount; i++)
                    context.Emit(OpCodes.Pop);                  // discard intermediates
                context.Emit(OpCodes.Br, label);                // jump with empty stack
                context.MarkLabel(skipTaken);                   // not-taken: reload cond... but we consumed it
                // Restore stack state for not-taken: push a dummy 0 back? No — cond was already consumed.
                // Actually: not-taken just continues with [..., intermediates...] on stack. ✓
                // We stored cond to local but for the not-taken path, cond is consumed (PopStackNoReturn already did it in tracking).
            }
            else
            {
                context.Emit(OpCodes.Brtrue, label);
            }
        }
        else
        {
            context.Emit(OpCodes.Brtrue, label);
        }

        // Code following br_if is conditionally reachable even if we were in unreachable mode.
        context.MarkReachable();
    }

    /// <summary>
    /// Provides a native representation of the instruction and the block index
    /// </summary>
    /// <returns>A string representation of this instance and the block index.</returns>
    public override string ToString() => $"{base.ToString()} {Index}";
}
