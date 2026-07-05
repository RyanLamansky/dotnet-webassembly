using System;
using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Re-throws an exception reference taken from the stack, preserving its identity (WASM 3.0 exception handling).
/// </summary>
public class ThrowRef : SimpleInstruction
{
    private static readonly System.Reflection.ConstructorInfo nullReferenceExceptionConstructor =
        typeof(NullReferenceException).GetConstructor(Type.EmptyTypes)!;

    /// <summary>
    /// Always <see cref="OpCode.ThrowRef"/>.
    /// </summary>
    public sealed override OpCode OpCode => OpCode.ThrowRef;

    /// <summary>
    /// Creates a new <see cref="ThrowRef"/> instance.
    /// </summary>
    public ThrowRef()
    {
    }

    internal sealed override void Compile(CompilationContext context)
    {
        // Pop an exnref-compatible reference (nullable exn). In unreachable code this is validation only.
        context.PopStackNoReturn(OpCode.ThrowRef, ValType.Ref(true, WebAssemblyHeapType.Exn));

        if (context.IsUnreachable)
        {
            context.MarkUnreachable();
            return;
        }

        // dup; brtrue OK; throw new NullReferenceException(); OK: castclass WebAssemblyException; throw;
        var ok = context.DefineLabel();
        context.Emit(OpCodes.Dup);
        context.Emit(OpCodes.Brtrue, ok);
        context.Emit(OpCodes.Newobj, nullReferenceExceptionConstructor);
        context.Emit(OpCodes.Throw);
        context.MarkLabel(ok);
        context.Emit(OpCodes.Castclass, context.Configuration.NeutralizeType(typeof(WebAssemblyException)));
        context.Emit(OpCodes.Throw);

        context.MarkUnreachable();
    }
}
