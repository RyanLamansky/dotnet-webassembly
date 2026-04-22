using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Signed remainder (result has the sign of the dividend).
/// </summary>
public class Int64RemainderSigned : ValueTwoToOneInstruction
{
    /// <summary>
    /// Always <see cref="OpCode.Int64RemainderSigned"/>.
    /// </summary>
    public sealed override OpCode OpCode => OpCode.Int64RemainderSigned;

    private protected sealed override WebAssemblyValueType ValueType => WebAssemblyValueType.Int64;

    private protected sealed override System.Reflection.Emit.OpCode EmittedOpCode => OpCodes.Rem;

    /// <summary>
    /// Creates a new  <see cref="Int64RemainderSigned"/> instance.
    /// </summary>
    public Int64RemainderSigned()
    {
    }

    internal sealed override void Compile(CompilationContext context)
    {
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int64, WebAssemblyValueType.Int64);
        context.Stack.Push(WebAssemblyValueType.Int64);
        context.Emit(OpCodes.Call, context[HelperMethod.Int64RemainderSigned, CreateHelper]);
    }

    private static MethodBuilder CreateHelper(HelperMethod helper, CompilationContext context)
    {
        var builder = context.CheckedExportsBuilder.DefineMethod(
            "☣ Int64RemainderSigned",
            CompilationContext.HelperMethodAttributes,
            typeof(long),
            [typeof(long), typeof(long)]);
        var il = builder.GetILGenerator();
        // WASM spec: INT64_MIN % -1 == 0 (CLR throws OverflowException otherwise)
        var normal = il.DefineLabel();
        il.Emit(OpCodes.Ldarg_1);
        il.Emit(OpCodes.Ldc_I4_M1);
        il.Emit(OpCodes.Conv_I8);
        il.Emit(OpCodes.Bne_Un_S, normal);
        il.Emit(OpCodes.Ldc_I4_0);
        il.Emit(OpCodes.Conv_I8);
        il.Emit(OpCodes.Ret);
        il.MarkLabel(normal);
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldarg_1);
        il.Emit(OpCodes.Rem);
        il.Emit(OpCodes.Ret);
        return builder;
    }
}
