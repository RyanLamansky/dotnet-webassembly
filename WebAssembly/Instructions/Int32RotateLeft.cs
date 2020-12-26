using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Sign-agnostic rotate left.
    /// </summary>
    public class Int32RotateLeft : SimpleInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.Int32RotateLeft"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Int32RotateLeft;

        /// <summary>
        /// Creates a new  <see cref="Int32RotateLeft"/> instance.
        /// </summary>
        public Int32RotateLeft()
        {
        }

        internal sealed override void Compile(CompilationContext context)
        {
            var stack = context.Stack;

            context.PopStackNoReturn(OpCode.Int32RotateLeft, WebAssemblyValueType.Int32, WebAssemblyValueType.Int32);
            stack.Push(WebAssemblyValueType.Int32);

            context.Emit(OpCodes.Call, context[HelperMethod.Int32RotateLeft, (helper, c) =>
            {
                var builder = c.CheckedExportsBuilder.DefineMethod(
                    "☣ Int32RotateLeft",
                    CompilationContext.HelperMethodAttributes,
                    typeof(uint),
                    new[]
                    {
                            typeof(uint),
                            typeof(int),
                    }
                    );

                var il = builder.GetILGenerator();
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Ldc_I4_S, 31);
                il.Emit(OpCodes.And);
                il.Emit(OpCodes.Shl);

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldc_I4_S, 32);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Sub);
                il.Emit(OpCodes.Ldc_I4_S, 31);
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