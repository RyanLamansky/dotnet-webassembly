using System;
using System.Reflection.Emit;

namespace WebAssembly.Runtime.Compilation;

/// <summary>
/// Shared IL emission for the WASM 3.0 tail-call instructions (<c>return_call</c> / <c>return_call_indirect</c>).
/// An IL <c>tail.</c> prefix requires the evaluation stack to hold exactly the call's arguments at the point of the
/// call, so any WASM operand-stack values sitting <em>below</em> the arguments (which a tail call discards) must be
/// cleared first. This spills the operands into locals and pops the remainder, leaving a clean stack for the caller
/// to reload the arguments in the order the call expects.
/// </summary>
internal static class TailCallHelper
{
    /// <summary>
    /// Spills the top <paramref name="types"/><c>.Length</c> evaluation-stack values (top-of-stack is the last
    /// element of <paramref name="types"/>) into fresh locals, then pops any values remaining beneath them — the
    /// values a tail call discards — leaving the evaluation stack empty of this instruction's operands.
    /// </summary>
    /// <param name="context">The active compilation context.</param>
    /// <param name="types">The operand types, in operand (bottom-to-top) order.</param>
    /// <param name="stackCountBefore">The evaluation-stack depth before the operands were consumed.</param>
    /// <returns>The spilled operands as locals, in operand order.</returns>
    /// <param name="discardBeneath">
    /// When true (the default), pops the values remaining beneath the operands. A tail call inside a protected region
    /// leaves instead of returning, and the <c>leave</c> clears the physical stack for free, so those pops are skipped
    /// (some of the beneath-values may also sit below the physical floor and must not be popped).
    /// </param>
    public static LocalBuilder[] SpillOperands(CompilationContext context, ReadOnlySpan<ValType> types, int stackCountBefore, bool discardBeneath = true)
    {
        var locals = new LocalBuilder[types.Length];
        for (var i = types.Length - 1; i >= 0; i--)
        {
            locals[i] = context.DeclareLocal(context.SystemType(types[i]));
            context.Emit(OpCodes.Stloc, locals[i]);
        }

        if (discardBeneath)
        {
            for (var i = 0; i < stackCountBefore - types.Length; i++)
                context.Emit(OpCodes.Pop);
        }

        return locals;
    }
}
