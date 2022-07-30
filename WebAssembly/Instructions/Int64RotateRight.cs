#if NETCOREAPP3_0_OR_GREATER
using System.Numerics;
using System.Reflection;
#endif
using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Sign-agnostic rotate right.
/// </summary>
public class Int64RotateRight : SimpleInstruction
{
    /// <summary>
    /// Always <see cref="OpCode.Int64RotateRight"/>.
    /// </summary>
    public sealed override OpCode OpCode => OpCode.Int64RotateRight;

    /// <summary>
    /// Creates a new  <see cref="Int64RotateRight"/> instance.
    /// </summary>
    public Int64RotateRight()
    {
    }

#if NETCOREAPP3_0_OR_GREATER
    private static readonly MethodInfo rotateRight = typeof(BitOperations).GetMethod(nameof(BitOperations.RotateRight), new[] { typeof(ulong), typeof(int) })!;
#endif

    internal sealed override void Compile(CompilationContext context)
    {
        var stack = context.Stack;

        context.PopStackNoReturn(OpCode.Int64RotateRight, WebAssemblyValueType.Int64, WebAssemblyValueType.Int64);
        stack.Push(WebAssemblyValueType.Int64);

#if NETCOREAPP3_0_OR_GREATER
        context.Emit(OpCodes.Conv_I4);
        context.Emit(OpCodes.Call, rotateRight);
#else
        context.Emit(OpCodes.Call, context[HelperMethod.Int64RotateRight, (helper, c) =>
        {
            var builder = c.CheckedExportsBuilder.DefineMethod(
                "☣ Int64RotateRight",
                CompilationContext.HelperMethodAttributes,
                typeof(ulong),
                new[]
                {
                            typeof(ulong),
                            typeof(long),
                }
                );

            var il = builder.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Conv_I4);
            il.Emit(OpCodes.Ldc_I4_S, 63);
            il.Emit(OpCodes.And);
            il.Emit(OpCodes.Shr_Un);

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldc_I4_S, 64);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Conv_I4);
            il.Emit(OpCodes.Sub);
            il.Emit(OpCodes.Ldc_I4_S, 63);
            il.Emit(OpCodes.And);
            il.Emit(OpCodes.Shl);
            il.Emit(OpCodes.Or);

            il.Emit(OpCodes.Ret);
            return builder;
        }
        ]);
#endif
    }
}
