using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Copysign.
    /// </summary>
    public class Float32CopySign : SimpleInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.Float32CopySign"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Float32CopySign;

        /// <summary>
        /// Creates a new  <see cref="Float32CopySign"/> instance.
        /// </summary>
        public Float32CopySign()
        {
        }

        internal sealed override void Compile(CompilationContext context)
        {
            context.PopStackNoReturn(OpCode.Float32CopySign, WebAssemblyValueType.Float32, WebAssemblyValueType.Float32);

            context.Emit(OpCodes.Call, context[HelperMethod.Float32CopySign, (helper, c) =>
            {
                var builder = c.CheckedExportsBuilder.DefineMethod(
                    "☣ Float32CopySign",
                    CompilationContext.HelperMethodAttributes,
                    typeof(float),
                    new[]
                    {
                        typeof(float),
                        typeof(float),
                    }
                    );

                var il = builder.GetILGenerator();
                var value = il.DeclareLocal(typeof(uint));

                il.Emit(OpCodes.Ldarga_S, 0);
                il.Emit(OpCodes.Ldind_U4);
                il.Emit(OpCodes.Ldc_I4, 0x7fffffff);
                il.Emit(OpCodes.And);
                il.Emit(OpCodes.Ldarga_S, 1);
                il.Emit(OpCodes.Ldind_U4);
                il.Emit(OpCodes.Ldc_I4, 0x80000000);
                il.Emit(OpCodes.And);
                il.Emit(OpCodes.Or);
                il.Emit(OpCodes.Stloc_0);
                il.Emit(OpCodes.Ldloca_S, value);
                il.Emit(OpCodes.Ldind_R4);
                il.Emit(OpCodes.Ret);

                return builder;
            }
            ]);

            context.Stack.Push(WebAssemblyValueType.Float32);
        }
    }
}