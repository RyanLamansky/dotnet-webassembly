using System.Reflection;
using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Signed remainder (result has the sign of the dividend).
/// </summary>
public class Int64RemainderSigned : SimpleInstruction
{
    /// <summary>
    /// Always <see cref="OpCode.Int64RemainderSigned"/>.
    /// </summary>
    public sealed override OpCode OpCode => OpCode.Int64RemainderSigned;

    /// <summary>
    /// Creates a new  <see cref="Int64RemainderSigned"/> instance.
    /// </summary>
    public Int64RemainderSigned()
    {
    }

    internal sealed override void Compile(CompilationContext context)
    {
        context.PopStackNoReturn(OpCode.Int64RemainderSigned, WebAssemblyValueType.Int64, WebAssemblyValueType.Int64);

        // A plain IL rem traps with OverflowException on long.MinValue % -1, but WebAssembly defines that as 0
        // (x % -1 == 0 for all x). Route through a helper that special-cases a -1 divisor; division by zero still
        // traps with DivideByZeroException, matching the "integer divide by zero" trap.
        context.Emit(OpCodes.Call, context[HelperMethod.Int64RemainderSigned, (helper, c) =>
        {
            var builder = c.CheckedExportsBuilder.DefineMethod(
                "☣ Int64RemainderSigned",
                CompilationContext.HelperMethodAttributes,
                typeof(long),
                [typeof(long), typeof(long)]
                );

            var il = builder.GetILGenerator();
            var divisorIsNegativeOne = il.DefineLabel();
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldc_I4_M1);
            il.Emit(OpCodes.Conv_I8);
            il.Emit(OpCodes.Beq_S, divisorIsNegativeOne);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Rem);
            il.Emit(OpCodes.Ret);
            il.MarkLabel(divisorIsNegativeOne);
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Conv_I8);
            il.Emit(OpCodes.Ret);

            // Tiny wrapper around a single rem; inlining lets the JIT fold it back to a bare remainder
            // in the common case where the divisor isn't -1.
            builder.SetImplementationFlags(MethodImplAttributes.AggressiveInlining);
            return builder;
        }
        ]);

        context.Stack.Push(WebAssemblyValueType.Int64);
    }
}
