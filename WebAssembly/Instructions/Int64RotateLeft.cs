using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Sign-agnostic rotate left.
    /// </summary>
    public class Int64RotateLeft : SimpleInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.Int64RotateLeft"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Int64RotateLeft;

        /// <summary>
        /// Creates a new  <see cref="Int64RotateLeft"/> instance.
        /// </summary>
        public Int64RotateLeft()
        {
        }

        internal sealed override void Compile(CompilationContext context)
        {
            var stack = context.Stack;

            context.PopStackNoReturn(OpCode.Int64RotateLeft, WebAssemblyValueType.Int64, WebAssemblyValueType.Int64);
            stack.Push(WebAssemblyValueType.Int64);

            context.Emit(OpCodes.Call, context[HelperMethod.Int64RotateLeft, (helper, c) =>
            {
                var builder = c.CheckedExportsBuilder.DefineMethod(
                    "☣ Int64RotateLeft",
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
                il.Emit(OpCodes.Shl);

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldc_I4_S, 64);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Conv_I4);
                il.Emit(OpCodes.Sub);
                il.Emit(OpCodes.Ldc_I4_S, 63);
                il.Emit(OpCodes.And);
                il.Emit(OpCodes.Shr_Un);
                il.Emit(OpCodes.Or);

                il.Emit(OpCodes.Ret);
                return builder;
            }
            ]);
        }
    }
}