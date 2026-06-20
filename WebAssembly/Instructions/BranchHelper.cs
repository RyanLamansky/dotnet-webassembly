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
    public static WebAssemblyValueType[] BranchTypes(CompilationContext context, int targetDepthKey, BlockContext targetBlockContext, bool isLoop)
    {
        if (targetBlockContext.BlockSignature is { } signature)
            return isLoop ? signature.RawParameterTypes : signature.RawReturnTypes;

        if (!isLoop && targetDepthKey == 1)
        {
            var functionReturns = context.CheckedSignature.RawReturnTypes;
            if (functionReturns.Length > 1)
                return functionReturns;
        }

        return Array.Empty<WebAssemblyValueType>();
    }

    /// <summary>
    /// Validates, without popping, that the top of the evaluation stack holds <paramref name="branchTypes"/> (the
    /// last type on top), and that they sit above the target block's baseline.
    /// </summary>
    public static void ValidateBranchTypes(CompilationContext context, OpCode opCode, WebAssemblyValueType[] branchTypes, BlockContext targetBlockContext)
    {
        var available = context.Stack.Count - targetBlockContext.InitialStackSize;
        if (available < branchTypes.Length)
            throw new StackSizeIncorrectException(opCode, branchTypes.Length, available);

        var snapshot = context.Stack.ToArray(); // Index 0 is the top of the stack.
        for (var k = 0; k < branchTypes.Length; k++)
        {
            var actual = snapshot[branchTypes.Length - 1 - k];
            if (actual != branchTypes[k])
                throw new StackTypeInvalidException(opCode, branchTypes[k], actual);
        }
    }

    /// <summary>
    /// In unreachable code a branch emits no IL but still type-checks the program, leaving the abstract stack as it
    /// found it.
    /// </summary>
    public static void ValidateUnreachable(CompilationContext context, OpCode opCode, BlockTypeInstruction blockType, WebAssemblyValueType[] branchTypes, bool isLoop, BlockContext targetBlockContext)
    {
        if (branchTypes.Length > 0)
        {
            var actualTypes = context.PopStack(opCode, branchTypes.Cast<WebAssemblyValueType?>().Reverse(), branchTypes.Length).ToArray();
            foreach (var actualType in actualTypes.AsEnumerable().Reverse())
                context.Stack.Push(actualType!.Value);
        }
        else if (!isLoop && targetBlockContext.BlockSignature == null && blockType.Type.TryToValueType(out var expectedType))
        {
            context.ValidateStack(opCode, expectedType);
        }
    }
}
