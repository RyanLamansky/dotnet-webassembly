using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Signed remainder (result has the sign of the dividend).
/// </summary>
public class Int32RemainderSigned : ValueTwoToOneInstruction
{
    /// <summary>
    /// Always <see cref="OpCode.Int32RemainderSigned"/>.
    /// </summary>
    public sealed override OpCode OpCode => OpCode.Int32RemainderSigned;

    private protected sealed override WebAssemblyValueType ValueType => WebAssemblyValueType.Int32;

    private protected sealed override System.Reflection.Emit.OpCode EmittedOpCode => OpCodes.Rem;

    /// <summary>
    /// Creates a new  <see cref="Int32RemainderSigned"/> instance.
    /// </summary>
    public Int32RemainderSigned()
    {
    }

    internal sealed override void Compile(CompilationContext context)
    {
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int32, WebAssemblyValueType.Int32);
        context.Stack.Push(WebAssemblyValueType.Int32);
        context.Emit(OpCodes.Call, context[HelperMethod.Int32RemainderSigned, CreateHelper]);
    }

    private static MethodBuilder CreateHelper(HelperMethod helper, CompilationContext context)
    {
        var builder = context.CheckedExportsBuilder.DefineMethod(
            "☣ Int32RemainderSigned",
            CompilationContext.HelperMethodAttributes,
            typeof(int),
            [typeof(int), typeof(int)]);
        var il = builder.GetILGenerator();
        // WASM spec: INT_MIN % -1 == 0 (CLR throws OverflowException otherwise)
        var normal = il.DefineLabel();
        il.Emit(OpCodes.Ldarg_1);
        il.Emit(OpCodes.Ldc_I4_M1);
        il.Emit(OpCodes.Bne_Un_S, normal);
        il.Emit(OpCodes.Ldc_I4_0);
        il.Emit(OpCodes.Ret);
        il.MarkLabel(normal);
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldarg_1);
        il.Emit(OpCodes.Rem);
        il.Emit(OpCodes.Ret);
        return builder;
    }
}
