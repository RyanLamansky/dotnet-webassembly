using System.Reflection.Emit;
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
        // Pop any reference type; push i32 result.
        context.PopStackNoReturn(OpCode.RefIsNull);
        context.Stack.Push(WebAssemblyValueType.Int32);

        context.Emit(OpCodes.Ldnull);
        context.Emit(OpCodes.Ceq);
    }
}
