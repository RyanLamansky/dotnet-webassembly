using System;
using System.Linq;
using System.Reflection;
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
                    var value = context.DeclareLocal(returns[0].ToSystemType());
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
            EmitMultiValueReturn(context, returns);
        }

        context.Emit(OpCodes.Ret);

        context.MarkUnreachable(functionWide: true);
    }

    internal static void EmitMultiValueReturn(CompilationContext context, WebAssemblyValueType[] returns)
    {
        var clrTypes = context.CheckedSignature.ReturnTypes;

        // Validate and consume the abstract value stack (last return is on top, so validate in reverse).
        context.PopStackNoReturn(OpCode.Return, returns.Cast<WebAssemblyValueType?>().Reverse(), returns.Length);

        // Store each value into a local (top of stack = last return).
        var locals = new LocalBuilder[returns.Length];
        for (var i = returns.Length - 1; i >= 0; i--)
        {
            locals[i] = context.DeclareLocal(clrTypes[i]);
            context.Emit(OpCodes.Stloc, locals[i]);
        }

        // Reload in order (first return first) for the ValueTuple constructor.
        for (var i = 0; i < returns.Length; i++)
            context.Emit(OpCodes.Ldloc, locals[i]);

        var tupleType = MultiValueHelper.ClrReturnType(clrTypes)!;
        var ctor = tupleType.GetConstructor(clrTypes)!;
        context.Emit(OpCodes.Newobj, ctor);
    }
}
