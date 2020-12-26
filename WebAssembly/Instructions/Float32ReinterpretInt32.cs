using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Reinterpret the bits of a 32-bit integer as a 32-bit float.
    /// </summary>
    public class Float32ReinterpretInt32 : SimpleInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.Float32ReinterpretInt32"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Float32ReinterpretInt32;

        /// <summary>
        /// Creates a new  <see cref="Float32ReinterpretInt32"/> instance.
        /// </summary>
        public Float32ReinterpretInt32()
        {
        }

        internal sealed override void Compile(CompilationContext context)
        {
            var stack = context.Stack;
            
            context.PopStackNoReturn(OpCode.Float32ReinterpretInt32, WebAssemblyValueType.Int32);

            stack.Push(WebAssemblyValueType.Float32);

            context.Emit(OpCodes.Call, context[HelperMethod.Float32ReinterpretInt32, (helper, c) =>
            {
                var builder = c.CheckedExportsBuilder.DefineMethod(
                    "☣ Float32ReinterpretInt32",
                    CompilationContext.HelperMethodAttributes,
                    typeof(float),
                    new[]
                    {
                            typeof(int),
                    }
                    );

                var il = builder.GetILGenerator();
                il.Emit(OpCodes.Ldarga_S, 0);
                il.Emit(OpCodes.Ldind_R4);
                il.Emit(OpCodes.Ret);
                return builder;
            }
            ]);
        }
    }
}