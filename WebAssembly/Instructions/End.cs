using System.Linq;
using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// An instruction that marks the end of a block, loop, if, or function.
/// </summary>
public class End : SimpleInstruction
{
    /// <summary>
    /// Always <see cref="OpCode.End"/>.
    /// </summary>
    public sealed override OpCode OpCode => OpCode.End;

    /// <summary>
    /// Creates a new <see cref="End"/> instance.
    /// </summary>
    public End()
    {
    }

    internal sealed override void Compile(CompilationContext context)
    {
        var stack = context.Stack;
        var blockType = context.Depth.Count == 0 ? BlockType.Empty : context.Depth.Peek().Type;

        if (context.Depth.Count == 1)
        {
            CompileFunctionEnd(context);
            return;
        }

        var blockContext = context.BlockContexts[context.Depth.Count];

        if (blockContext.BlockSignature != null)
        {
            // Multi-value block: validate the results and ferry them through result locals so the end label is
            // reached with a clean stack on both the fall-through path and any branch paths.
            var returns = blockContext.BlockSignature.RawReturnTypes;
            var expected = blockContext.InitialStackSize + returns.Length;
            if (stack.Count != expected && (!context.IsUnreachable || stack.Count > expected))
                throw new StackSizeIncorrectException(OpCode.End, expected, stack.Count);

            // An if-without-else whose parameters equal its results is an implicit pass-through: both the then and
            // the (absent) else path naturally leave the same values, so no ferrying is needed.
            var ifNoElsePassThrough = blockContext.IfFalseLabel.HasValue
                && blockContext.BlockSignature.RawParameterTypes.SequenceEqual(returns);

            if (!context.IsUnreachable && !ifNoElsePassThrough && returns.Length > 0)
            {
                blockContext.ResultLocals ??= context.DeclareResultLocals(returns);
                for (var i = returns.Length - 1; i >= 0; i--)
                    context.Emit(OpCodes.Stloc, blockContext.ResultLocals[i]);
            }
        }
        else if (blockType.TryToValueType(out var expectedType))
        {
            var expected = blockContext.InitialStackSize + 1;
            if (stack.Count != expected && (!context.IsUnreachable || stack.Count > expected))
                throw new StackSizeIncorrectException(OpCode.End, expected, stack.Count);

            context.ValidateStack(OpCode.End, expectedType);

            // Stash the fall-through result so the end label is reached with a clean stack; it (and any branch
            // path, which uses the same local) reloads after the label.
            if (!context.IsUnreachable)
                context.Emit(OpCodes.Stloc, context.GetOrCreateResultLocal(0, expectedType));
        }
        else if (stack.Count != blockContext.InitialStackSize && (!context.IsUnreachable || stack.Count > blockContext.InitialStackSize))
        {
            throw new StackSizeIncorrectException(OpCode.End, blockContext.InitialStackSize, stack.Count);
        }

        // An if-without-else: the false path falls through to here. It is only valid when the block produces no net
        // result (results equal parameters), since the false path supplies no result values.
        if (blockContext.IfFalseLabel.HasValue)
        {
            if (blockContext.BlockSignature != null)
            {
                var parameters = blockContext.BlockSignature.RawParameterTypes;
                var returns = blockContext.BlockSignature.RawReturnTypes;
                if (!parameters.SequenceEqual(returns))
                    throw new StackSizeIncorrectException(OpCode.End, blockContext.InitialStackSize + returns.Length, blockContext.InitialStackSize + parameters.Length);
            }
            else if (blockType.TryToValueType(out _))
            {
                throw new StackSizeIncorrectException(OpCode.End, blockContext.InitialStackSize + 1, blockContext.InitialStackSize);
            }

            context.MarkLabel(blockContext.IfFalseLabel.Value);

            // If a branch targeted this block before its end (allocating result locals), the false path arrives with
            // the pass-through values on the stack; stash them so the exit label has the same clean stack as the
            // true/branch paths.
            if (blockContext.ResultLocals != null)
            {
                var returns = blockContext.BlockSignature!.RawReturnTypes;
                for (var i = returns.Length - 1; i >= 0; i--)
                    context.Emit(OpCodes.Stloc, blockContext.ResultLocals[i]);
            }
            else if (blockContext.ResultLocal != null)
            {
                context.Emit(OpCodes.Stloc, blockContext.ResultLocal);
            }
        }

        context.BlockContexts.Remove(context.Depth.Count);
        context.Depth.PopNoReturn();

        var depth = checked((uint)context.Depth.Count);
        var label = context.Labels[depth];

        if (!context.LoopLabels.Remove(label)) //Loop labels are marked where defined.
            context.MarkLabel(label);

        context.Labels.Remove(depth);

        // Completing a structured construct restores the parent's stack to its state at block entry plus the
        // construct's results, and re-enables reachability (the end label is a join point). SectionData removes the
        // block context manually, so only adjust when the parent context still exists.
        if (context.BlockContexts.TryGetValue(context.Depth.Count, out _))
        {
            while (stack.Count > blockContext.InitialStackSize)
                stack.Pop();

            if (blockContext.BlockSignature != null)
            {
                foreach (var t in blockContext.BlockSignature.RawReturnTypes)
                    stack.Push(t);
            }
            else if (blockType.TryToValueType(out var blockResultType))
            {
                stack.Push(blockResultType);
            }

            context.MarkReachable();
        }

        // Reload the ferried results after the label so the parent continues with them on the evaluation stack.
        if (blockContext.ResultLocals != null)
        {
            foreach (var local in blockContext.ResultLocals)
                context.Emit(OpCodes.Ldloc, local);
        }
        else if (blockContext.ResultLocal != null)
        {
            context.Emit(OpCodes.Ldloc, blockContext.ResultLocal);
        }
    }

    static void CompileFunctionEnd(CompilationContext context)
    {
        var stack = context.Stack;
        var functionBlock = context.BlockContexts[1];
        var returns = context.CheckedSignature.RawReturnTypes;
        var returnsLength = returns.Length;

        if (returnsLength < stack.Count || (returnsLength > stack.Count && !context.IsUnreachable))
            throw new StackSizeIncorrectException(OpCode.End, returnsLength, stack.Count);

        if (returnsLength == 0)
        {
            context.MarkLabel(context.Labels[0]);
        }
        else if (returnsLength == 1)
        {
            var popped = context.PopStack(OpCode.End, returns[0]);
            if (!popped.HasValue && !context.IsUnreachable)
                throw new OpCodeCompilationException(OpCode.End, "Cannot determine stack type.");

            // Ferry the single result through a local so the fall-through end and any branch-to-function-return
            // converge at the label, then reload it for the Ret.
            if (!context.IsUnreachable)
            {
                var resultLocal = functionBlock.ResultLocal ?? context.GetOrCreateResultLocal(0, returns[0]);
                context.Emit(OpCodes.Stloc, resultLocal);
                context.MarkLabel(context.Labels[0]);
                context.Emit(OpCodes.Ldloc, resultLocal);
            }
            else if (functionBlock.ResultLocal != null)
            {
                // The body falls through unreachable, but a branch stashed the result; reload it at the label.
                context.MarkLabel(context.Labels[0]);
                context.Emit(OpCodes.Ldloc, functionBlock.ResultLocal);
            }
            else
            {
                context.MarkLabel(context.Labels[0]);
            }
        }
        else
        {
            if (!context.IsUnreachable)
            {
                functionBlock.ResultLocals ??= context.DeclareResultLocals(returns);
                for (var i = returnsLength - 1; i >= 0; i--)
                    context.Emit(OpCodes.Stloc, functionBlock.ResultLocals[i]);
            }

            context.MarkLabel(context.Labels[0]);

            if (functionBlock.ResultLocals != null)
            {
                foreach (var local in functionBlock.ResultLocals)
                    context.Emit(OpCodes.Ldloc, local);

                MultiValueHelper.EmitTuplePack(context, context.CheckedSignature.ReturnTypes);
            }
        }

        context.Emit(OpCodes.Ret);
    }
}
