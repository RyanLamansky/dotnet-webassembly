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
            var functionBlockCtx = context.BlockContexts[1];
            var returns = context.CheckedSignature.RawReturnTypes;
            var returnsLength = returns.Length;

            if (returnsLength < stack.Count || (returnsLength > stack.Count && !context.IsUnreachable))
                throw new StackSizeIncorrectException(OpCode.End, returnsLength, stack.Count);

            if (returnsLength == 1)
            {
                // Validate type (throws on mismatch even in unreachable mode for correctness).
                var popped = context.PopStack(OpCode.End, returns[0]);
                if (!popped.HasValue && !context.IsUnreachable)
                    throw new OpCodeCompilationException(OpCode.End, "Cannot determine stack type.");

                // Use result-local pattern so br-to-function-block and fall-through both work:
                // fall-through stlocs here; branch already stloc'd; both ldloc after markLabel.
                if (!context.IsUnreachable)
                {
                    var resultLocal = functionBlockCtx.ResultLocal
                        ?? context.GetOrCreateResultLocal(0, returns[0]);
                    context.Emit(OpCodes.Stloc, resultLocal);
                    context.MarkLabel(context.Labels[0]);
                    context.Emit(OpCodes.Ldloc, resultLocal);
                }
                else if (functionBlockCtx.ResultLocal != null)
                {
                    // A br targeted the function outer block; reload its stashed result.
                    context.MarkLabel(context.Labels[0]);
                    context.Emit(OpCodes.Ldloc, functionBlockCtx.ResultLocal);
                }
                else
                {
                    // All paths return via `return` instruction; nothing to emit at label.
                    context.MarkLabel(context.Labels[0]);
                }
            }
            else
            {
                if (returnsLength > 1)
                    Return.EmitMultiValueReturn(context, returns);
                context.MarkLabel(context.Labels[0]);
            }

            context.Emit(OpCodes.Ret);
        }
        else
        {
            var blockContext = context.BlockContexts[context.Depth.Count];

            if (blockContext.BlockSignature != null)
            {
                // Multi-value TypeIndex block: validate and stash all results.
                var returns = blockContext.BlockSignature.RawReturnTypes;
                var expected = blockContext.InitialStackSize + returns.Length;
                if (stack.Count != expected && (!context.IsUnreachable || stack.Count > expected))
                    throw new StackSizeIncorrectException(OpCode.End, expected, stack.Count);

                // For if-without-else (IfFalseLabel still set) with params≡results: don't stash/reload.
                // Both paths naturally leave the same values on the IL stack.
                var isIfNoElsePassThrough = blockContext.IfFalseLabel.HasValue
                    && blockContext.BlockSignature.RawParameterTypes.SequenceEqual(returns);

                if (!context.IsUnreachable && !isIfNoElsePassThrough)
                {
                    // Allocate result locals if not yet created.
                    if (blockContext.ResultLocals == null)
                    {
                        blockContext.ResultLocals = new LocalBuilder[returns.Length];
                        for (var i = 0; i < returns.Length; i++)
                            blockContext.ResultLocals[i] = context.DeclareLocal(returns[i].ToSystemType());
                    }
                    // Stash from top-of-stack down (last result first).
                    for (var i = returns.Length - 1; i >= 0; i--)
                        context.Emit(OpCodes.Stloc, blockContext.ResultLocals[i]);
                }
            }
            else if (blockType.TryToValueType(out var expectedType))
            {
                context.ValidateStack(OpCode.End, expectedType);

                // Stash fall-through value to result local (so the label target has an empty IL stack),
                // then reload it after marking the label. The same local is used when a br/br_if jumps
                // to this block, so both paths converge correctly.
                // Only allocate if reachable (fall-through produces a value); if unreachable, only use
                // an already-allocated local (set by a prior br to this block).
                if (!context.IsUnreachable)
                {
                    var resultLocal = context.GetOrCreateResultLocal(0, expectedType);
                    context.Emit(OpCodes.Stloc, resultLocal);
                }
            }
            else if (stack.Count != blockContext.InitialStackSize && (!context.IsUnreachable || stack.Count > blockContext.InitialStackSize))
            {
                throw new StackSizeIncorrectException(OpCode.End, blockContext.InitialStackSize, stack.Count);
            }

            // For if-without-else: validate the block is void-equivalent (results must equal params for TypeIndex,
            // or be void for inline types). If valid, mark the false-branch label (jumps here on false condition).
            if (blockContext.IfFalseLabel.HasValue)
            {
                if (blockContext.BlockSignature != null)
                {
                    // TypeIndex if-without-else: valid only if params == results (implicit pass-through).
                    var p = blockContext.BlockSignature.RawParameterTypes;
                    var r = blockContext.BlockSignature.RawReturnTypes;
                    if (!p.SequenceEqual(r))
                        throw new StackSizeIncorrectException(OpCode.End, blockContext.InitialStackSize + r.Length, blockContext.InitialStackSize + p.Length);
                }
                else if (blockType.TryToValueType(out _))
                    throw new StackSizeIncorrectException(OpCode.End, blockContext.InitialStackSize + 1, blockContext.InitialStackSize);
                context.MarkLabel(blockContext.IfFalseLabel.Value);

                // If a br targeted this block before the else/end (allocating ResultLocals),
                // the false-path arrives here with params on the IL stack. Stash them so the
                // exit label has a clean stack, matching the true/br path.
                if (blockContext.BlockSignature != null && blockContext.ResultLocals != null)
                {
                    var returns = blockContext.BlockSignature.RawReturnTypes;
                    for (var i = returns.Length - 1; i >= 0; i--)
                        context.Emit(OpCodes.Stloc, blockContext.ResultLocals[i]);
                }
                else if (blockContext.ResultLocal != null)
                    context.Emit(OpCodes.Stloc, blockContext.ResultLocal);
            }

            var wasUnreachable = context.IsUnreachable;

            context.BlockContexts.Remove(context.Depth.Count);
            context.Depth.PopNoReturn();

            var depth = checked((uint)context.Depth.Count);
            var label = context.Labels[depth];

            var isLoopLabel = context.LoopLabels.Remove(label);
            if (!isLoopLabel)
                context.MarkLabel(label);

            context.Labels.Remove(depth);

            // A non-loop block exit label is reachable from here only when control can fall through
            // to it, or when a reachable branch targeted this block's end label.
            // When the block was unreachable, reset the tracking stack (unreachable code may have left
            // stale values) to represent the correct state: parent's initial items plus the block result.
            // Only applies when the parent block context still exists (SectionData removes it manually).
            if (!isLoopLabel && context.BlockContexts.TryGetValue(context.Depth.Count, out _))
            {
                if (!wasUnreachable || blockContext.IsEndLabelTargeted)
                {
                    context.MarkReachable();
                    if (wasUnreachable)
                    {
                        while (context.Stack.Count > blockContext.InitialStackSize)
                            context.Stack.Pop();
                        // Push results onto the tracking stack.
                        if (blockContext.BlockSignature != null)
                        {
                            foreach (var t in blockContext.BlockSignature.RawReturnTypes)
                                context.Stack.Push(t);
                        }
                        else if (blockType.TryToValueType(out var blockResultType))
                            context.Stack.Push(blockResultType);
                    }
                }
                else
                {
                    context.MarkUnreachable();
                    if (blockContext.BlockSignature != null)
                    {
                        foreach (var t in blockContext.BlockSignature.RawReturnTypes)
                            context.Stack.Push(t);
                    }
                    else if (blockType.TryToValueType(out var blockResultType))
                        context.Stack.Push(blockResultType);
                }
            }

            // Reload result values after the label (both fall-through and branch paths arrive here).
            if (blockContext.ResultLocals != null)
            {
                foreach (var local in blockContext.ResultLocals)
                    context.Emit(OpCodes.Ldloc, local);
            }
            else if (blockContext.ResultLocal != null)
                context.Emit(OpCodes.Ldloc, blockContext.ResultLocal);
        }
    }
}
