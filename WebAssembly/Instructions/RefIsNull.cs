using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Test whether a reference value is null; pushes 1 (i32) if null, 0 otherwise.
/// </summary>
public class RefIsNull : SimpleInstruction
{
    /// <summary>
    /// Always <see cref="OpCode.RefIsNull"/>.
    /// </summary>
    public sealed override OpCode OpCode => OpCode.RefIsNull;

    /// <summary>
    /// Creates a new <see cref="RefIsNull"/> instance.
    /// </summary>
    public RefIsNull()
    {
    }

    internal sealed override void Compile(CompilationContext context)
    {
        // The operand must be a reference type (funcref or externref); anything else would emit a
        // nonsensical `ceq` comparing a non-reference value against null. A null type means the value
        // is unknown because the code is unreachable, which is accepted.
        var type = context.PopStack(OpCode.RefIsNull, null);
        if (type is not (null or WebAssemblyValueType.FuncRef or WebAssemblyValueType.ExternRef))
            throw new StackTypeInvalidException(OpCode.RefIsNull, WebAssemblyValueType.FuncRef, type.Value);

        context.Stack.Push(WebAssemblyValueType.Int32);

        context.Emit(OpCodes.Ldnull);
        context.Emit(OpCodes.Ceq);
    }
}
