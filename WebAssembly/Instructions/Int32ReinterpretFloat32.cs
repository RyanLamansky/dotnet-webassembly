using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Reinterpret the bits of a 32-bit float as a 32-bit integer.
    /// </summary>
    public class Int32ReinterpretFloat32 : SimpleInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.Int32ReinterpretFloat32"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Int32ReinterpretFloat32;

        /// <summary>
        /// Creates a new  <see cref="Int32ReinterpretFloat32"/> instance.
        /// </summary>
        public Int32ReinterpretFloat32()
        {
        }

        internal sealed override void Compile(CompilationContext context)
        {
            var stack = context.Stack;

            context.PopStackNoReturn(OpCode.Int32ReinterpretFloat32, WebAssemblyValueType.Float32);

            stack.Push(WebAssemblyValueType.Int32);

            context.Emit(OpCodes.Call, context[HelperMethod.Int32ReinterpretFloat32, (helper, c) =>
            {
                var builder = c.CheckedExportsBuilder.DefineMethod(
                    "☣ Int32ReinterpretFloat32",
                    CompilationContext.HelperMethodAttributes,
                    typeof(int),
                    new[]
                    {
                            typeof(float),
                    }
                    );

                var il = builder.GetILGenerator();
                il.Emit(OpCodes.Ldarga_S, 0);
                il.Emit(OpCodes.Ldind_I4);
                il.Emit(OpCodes.Ret);
                return builder;
            }
            ]);
        }
    }
}