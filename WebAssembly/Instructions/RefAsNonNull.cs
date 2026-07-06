using System;
using System.Reflection;
using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Convert a nullable reference to a non-null reference of the same heap type, trapping if the value is null
/// (WebAssembly 3.0 typed function references).
/// </summary>
public class RefAsNonNull : SimpleInstruction
{
    private static readonly ConstructorInfo nullReferenceExceptionConstructor =
        typeof(NullReferenceException).GetConstructor(Type.EmptyTypes)!;

    /// <summary>
    /// Always <see cref="OpCode.RefAsNonNull"/>.
    /// </summary>
    public sealed override OpCode OpCode => OpCode.RefAsNonNull;

    /// <summary>
    /// Creates a new <see cref="RefAsNonNull"/> instance.
    /// </summary>
    public RefAsNonNull()
    {
    }

    internal sealed override void Compile(CompilationContext context)
    {
        var popped = context.PopStack(OpCode.RefAsNonNull, null);
        if (popped is { } operand)
        {
            if (!operand.IsReference)
                throw new StackTypeInvalidException(OpCode.RefAsNonNull, WebAssemblyValueType.FuncRef, operand.ToNarrow());

            var nonNull = operand.IsIndexed
                ? ValType.RefIndexed(false, operand.TypeIndex)
                : ValType.Ref(false, operand.HeapType);
            context.Stack.Push(nonNull);
        }
        else
        {
            context.Stack.Push(null); // Unknown (unreachable code) stays unknown.
        }

        // In unreachable code this is validation only; a dead `dup` on a provably-empty IL stack is worth avoiding.
        if (context.IsUnreachable)
            return;

        // dup; brtrue CONT; throw new NullReferenceException(); CONT:
        var cont = context.DefineLabel();
        context.Emit(OpCodes.Dup);
        context.Emit(OpCodes.Brtrue, cont);
        context.Emit(OpCodes.Newobj, nullReferenceExceptionConstructor);
        context.Emit(OpCodes.Throw);
        context.MarkLabel(cont);
    }
}
