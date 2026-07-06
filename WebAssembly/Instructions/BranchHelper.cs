using System;
using System.Linq;
using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Shared logic for the branch instructions (<see cref="Branch"/>, <see cref="BranchIf"/>, <see cref="BranchTable"/>)
/// concerning the values a branch carries to its target and the stack validation around them.
/// </summary>
static class BranchHelper
{
    /// <summary>
    /// Pops the carried values off the top of the evaluation stack into <paramref name="locals"/>, leaving the stack
    /// at the target block's baseline. The first result (index 0, deepest on the stack) lands in the first local, so
    /// <see cref="ReloadResults"/> restores the original order.
    /// </summary>
    public static void StashResults(CompilationContext context, LocalBuilder[] locals)
    {
        for (var i = locals.Length - 1; i >= 0; i--)
            context.Emit(OpCodes.Stloc, locals[i]);
    }

    /// <summary>
    /// Pushes the values previously saved by <see cref="StashResults"/> back onto the evaluation stack in their
    /// original order (first result deepest, last result on top).
    /// </summary>
    public static void ReloadResults(CompilationContext context, LocalBuilder[] locals)
    {
        foreach (var local in locals)
            context.Emit(OpCodes.Ldloc, local);
    }

    /// <summary>
    /// Pops <paramref name="count"/> intermediate values off the evaluation stack and discards them, used to clear
    /// values left beneath a branch's carried results before jumping.
    /// </summary>
    public static void DiscardIntermediates(CompilationContext context, int count)
    {
        for (var i = 0; i < count; i++)
            context.Emit(OpCodes.Pop);
    }

    /// <summary>
    /// The multi-value types a branch to the target carries: a loop's parameters, a block/if's results, or — at the
    /// function boundary — the function's results when there are two or more. Empty for void and single-value
    /// targets, which the branch instructions handle through the simpler inline paths.
    /// </summary>
    public static ValType[] BranchTypes(CompilationContext context, int targetDepthKey, BlockContext targetBlockContext, bool isLoop)
    {
        if (targetBlockContext.BlockSignature is { } signature)
            return isLoop ? signature.RawParameterTypes : signature.RawReturnTypes;

        if (!isLoop && targetDepthKey == 1)
        {
            var functionReturns = context.CheckedSignature.RawReturnTypes;
            if (functionReturns.Length > 1)
                return functionReturns;
        }

        return Array.Empty<ValType>();
    }

    /// <summary>
    /// The single carried result type for a target with exactly one result not handled by <see cref="BranchTypes"/>
    /// (which returns the multi-value list): an inline <see cref="BlockType"/> (numeric or shorthand reference) or a
    /// function boundary whose single result is a long-form/indexed reference the outer block's <see cref="BlockType"/>
    /// cannot express. Returns false for a void or multi-value target.
    /// </summary>
    public static bool TrySingleResultType(CompilationContext context, BlockTypeInstruction blockType, int targetDepthKey, bool isLoop, out ValType type)
    {
        if (!isLoop && blockType.Type.TryToValueType(out var narrow))
        {
            type = ValType.FromValueType(narrow);
            return true;
        }

        if (!isLoop && targetDepthKey == 1)
        {
            var returns = context.CheckedSignature.RawReturnTypes;
            if (returns.Length == 1)
            {
                type = returns[0];
                return true;
            }
        }

        type = default;
        return false;
    }

    /// <summary>
    /// Validates, without popping, that the top of the evaluation stack holds <paramref name="branchTypes"/> (the
    /// last type on top), and that they sit above the target block's baseline.
    /// </summary>
    public static void ValidateBranchTypes(CompilationContext context, OpCode opCode, ValType[] branchTypes, BlockContext targetBlockContext)
    {
        // The carried values must be available above the *current* control frame's baseline — a branch cannot
        // reach values hidden beneath the innermost enclosing block, even when the target is an outer block whose
        // baseline is lower (br_table.10 `type-arg-void-vs-num-nested`).
        var currentBaseline = context.BlockContexts[context.Depth.Count].InitialStackSize;
        var available = context.Stack.Count - currentBaseline;
        if (available < branchTypes.Length)
            throw new StackSizeIncorrectException(opCode, branchTypes.Length, available);

        var snapshot = context.Stack.ToArray(); // Index 0 is the top of the stack.
        for (var k = 0; k < branchTypes.Length; k++)
        {
            var actual = snapshot[branchTypes.Length - 1 - k];
            // An unknown (null) entry is the polymorphic value from unreachable code; it matches any branch type.
            // The carried value must be a subtype of the label's expected type (equality for numerics).
            if (actual.HasValue && !context.IsSubtype(actual.Value, branchTypes[k]))
                throw new StackTypeInvalidException(opCode, branchTypes[k].ToNarrow(), actual.Value.ToNarrow());
        }
    }

    /// <summary>
    /// Emits, for reachable code, the value-ferrying and intermediate-discarding for a branch to the target label
    /// (short of the final <c>br</c> itself): stashing multi-value results into the target's result locals, ferrying
    /// a single inline-typed result, or clearing a void target's intermediates. Shared by <see cref="Branch"/> and
    /// the reference branch instructions (<see cref="BranchOnNull"/>/<see cref="BranchOnNonNull"/>).
    /// </summary>
    public static void EmitReachableBranch(CompilationContext context, OpCode opCode, int index, BlockTypeInstruction blockType, BlockContext targetBlockContext, bool isLoop, ValType[] branchTypes)
    {
        if (branchTypes.Length > 0)
        {
            ValidateBranchTypes(context, opCode, branchTypes, targetBlockContext);

            var discardCount = context.Stack.Count - targetBlockContext.InitialStackSize - branchTypes.Length;

            if (!isLoop)
            {
                targetBlockContext.ResultLocals ??= context.DeclareResultLocals(branchTypes);
                StashResults(context, targetBlockContext.ResultLocals);
                DiscardIntermediates(context, discardCount);
            }
            else if (discardCount > 0)
            {
                var temporaries = context.DeclareResultLocals(branchTypes);
                StashResults(context, temporaries);
                DiscardIntermediates(context, discardCount);
                ReloadResults(context, temporaries);
            }
        }
        else if (TrySingleResultType(context, blockType, context.Depth.Count - index, isLoop, out var expectedType))
        {
            context.ValidateStack(opCode, expectedType);
            context.Emit(OpCodes.Stloc, context.GetOrCreateResultLocal(index, expectedType));

            var intermediateCount = context.Stack.Count - targetBlockContext.InitialStackSize - 1;
            DiscardIntermediates(context, intermediateCount);
        }
        else
        {
            var discardCount = context.Stack.Count - targetBlockContext.InitialStackSize;
            DiscardIntermediates(context, discardCount);
        }
    }

    /// <summary>
    /// The single choke point for a reachable WASM branch's control transfer (<see cref="Branch"/>,
    /// <see cref="BranchOnNull"/>, <see cref="BranchOnNonNull"/>). When the target lies within the current protected
    /// region — always the case for a module without exception handling — this is the byte-identical existing
    /// emission (<see cref="EmitReachableBranch"/> then <c>br</c>). Otherwise it ferries the carried values and emits
    /// a single <c>leave</c> to the crossed region's exit stub (or its resume point for the region's own end).
    /// </summary>
    public static void EmitBranchTransfer(CompilationContext context, OpCode opCode, int index, BlockTypeInstruction blockType, int targetDepthKey, BlockContext targetBlockContext, bool isLoop, ValType[] branchTypes, Label label)
    {
        var region = context.CrossingRegion(targetDepthKey);
        if (region == null)
        {
            EmitReachableBranch(context, opCode, index, blockType, targetBlockContext, isLoop, branchTypes);
            context.Emit(OpCodes.Br, label);
            return;
        }

        // Crossing a region: ferry carried values into locals (never discarding intermediates — leave clears the
        // physical stack), then leave to the region's resume point (own end) or an exit stub (an outer target).
        var ownEnd = region.DepthKey == targetDepthKey;
        ProtectedRegion.ExitStub? stub = ownEnd
            ? null
            : context.GetOrCreateExitStub(region, targetDepthKey, label, targetBlockContext.InitialStackSize, isLoop, branchTypes);

        if (branchTypes.Length > 0)
        {
            ValidateBranchTypes(context, opCode, branchTypes, targetBlockContext);
            if (!isLoop)
            {
                targetBlockContext.ResultLocals ??= context.DeclareResultLocals(branchTypes);
                StashResults(context, targetBlockContext.ResultLocals);
            }
            else
            {
                StashResults(context, stub!.CarriedLocals!);
            }
        }
        else if (TrySingleResultType(context, blockType, targetDepthKey, isLoop, out var expectedType))
        {
            context.ValidateStack(opCode, expectedType);
            context.Emit(OpCodes.Stloc, context.GetOrCreateResultLocal(index, expectedType));
        }

        context.Emit(OpCodes.Leave, ownEnd ? region.EhEndLabel : stub!.StubLabel);
    }

    /// <summary>
    /// The dead-code counterpart to <see cref="EmitBranchTransfer"/>: no values are ferried, but a branch inside a
    /// protected region must still be structurally valid IL, so a crossing branch emits <c>leave</c> (to the region's
    /// resume point or exit stub) rather than an illegal <c>br</c> across the region boundary.
    /// </summary>
    public static void EmitDeadBranchTransfer(CompilationContext context, int targetDepthKey, BlockContext targetBlockContext, bool isLoop, ValType[] branchTypes, Label label)
    {
        var region = context.CrossingRegion(targetDepthKey);
        if (region == null)
        {
            context.Emit(OpCodes.Br, label);
            return;
        }

        if (region.DepthKey == targetDepthKey)
        {
            context.Emit(OpCodes.Leave, region.EhEndLabel);
            return;
        }

        var stub = context.GetOrCreateExitStub(region, targetDepthKey, label, targetBlockContext.InitialStackSize, isLoop, branchTypes);
        context.Emit(OpCodes.Leave, stub.StubLabel);
    }

    /// <summary>
    /// In unreachable code a branch emits no IL but still type-checks the program, leaving the abstract stack as it
    /// found it.
    /// </summary>
    public static void ValidateUnreachable(CompilationContext context, OpCode opCode, BlockTypeInstruction blockType, ValType[] branchTypes, bool isLoop, BlockContext targetBlockContext)
    {
        if (branchTypes.Length > 0)
        {
            var actualTypes = context.PopStack(opCode, branchTypes.Select(t => (ValType?)t).Reverse(), branchTypes.Length).ToArray();
            foreach (var actualType in actualTypes.AsEnumerable().Reverse())
                context.Stack.Push(actualType!.Value);
        }
        else if (!isLoop && targetBlockContext.BlockSignature == null && blockType.Type.TryToValueType(out var expectedType))
        {
            context.ValidateStack(opCode, expectedType);
        }
    }
}
