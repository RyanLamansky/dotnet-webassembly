using System.Linq;
using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Return zero or more values from this function.
/// </summary>
public class Return : SimpleInstruction
{
    /// <summary>
    /// Always <see cref="OpCode.Return"/>.
    /// </summary>
    public sealed override OpCode OpCode => OpCode.Return;

    /// <summary>
    /// Creates a new  <see cref="Return"/> instance.
    /// </summary>
    public Return()
    {
    }

    internal sealed override void Compile(CompilationContext context)
    {
        var returns = context.CheckedSignature.RawReturnTypes;
        var stack = context.Stack;

        var returnsLength = returns.Length;
        var stackCount = stack.Count;

        // Inside a protected region a `ret` is illegal IL: instead stash the results into the function block's result
        // locals (which CompileFunctionEnd reloads) and `leave` to the function-end label, which sits outside every
        // region. A single `leave` clears the physical stack and exits all enclosing regions.
        if (context.InProtectedRegion)
        {
            var functionBlock = context.BlockContexts[1];
            if (returnsLength == 1)
            {
                context.PopStackNoReturn(OpCode.Return, returns[0]);
                context.Emit(OpCodes.Stloc, functionBlock.ResultLocal ??= context.DeclareLocal(context.SystemType(returns[0])));
            }
            else if (returnsLength > 1)
            {
                context.PopStackNoReturn(OpCode.Return, returns.Select(t => (ValType?)t).Reverse(), returnsLength);
                functionBlock.ResultLocals ??= context.DeclareResultLocals(returns);
                BranchHelper.StashResults(context, functionBlock.ResultLocals);
            }

            context.Emit(OpCodes.Leave, context.Labels[0]);
            context.MarkUnreachable(functionWide: true);
            return;
        }

        if (returnsLength <= 1)
        {
            if (stackCount > returnsLength)
            {
                if (returnsLength == 0)
                {
                    for (var i = 0; i < stackCount - returnsLength; i++)
                        context.Emit(OpCodes.Pop);
                }
                else
                {
                    var value = context.DeclareLocal(context.SystemType(returns[0]));
                    context.Emit(OpCodes.Stloc, value.LocalIndex);

                    for (var i = 0; i < stackCount - returnsLength; i++)
                        context.Emit(OpCodes.Pop);

                    context.Emit(OpCodes.Ldloc, value.LocalIndex);
                }
            }

            if (returnsLength == 1)
                context.PopStackNoReturn(OpCode.Return, returns[0]);
        }
        else
        {
            // Validate (last return is on top, so check in reverse) and pack the results into a ValueTuple.
            context.PopStackNoReturn(OpCode.Return, returns.Select(t => (ValType?)t).Reverse(), returnsLength);

            var clrTypes = context.CheckedSignature.ReturnTypes;
            MultiValueHelper.EmitTuplePack(context, clrTypes);

            // Any values beneath the results must be discarded before the tuple can be returned.
            if (stackCount > returnsLength)
            {
                var tuple = context.DeclareLocal(MultiValueHelper.ClrReturnType(clrTypes)!);
                context.Emit(OpCodes.Stloc, tuple.LocalIndex);

                for (var i = 0; i < stackCount - returnsLength; i++)
                    context.Emit(OpCodes.Pop);

                context.Emit(OpCodes.Ldloc, tuple.LocalIndex);
            }
        }

        context.Emit(OpCodes.Ret);

        //Mark the subsequent code within this function is unreachable
        context.MarkUnreachable(functionWide: true);
    }
}
