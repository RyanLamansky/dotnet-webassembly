using System.Reflection.Emit;
using WebAssembly.Runtime;
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
            if (stack.Count < 1)
                throw new StackTooSmallException(OpCode.Int32ReinterpretFloat32, 1, stack.Count);

            var type = stack.Pop();
            if (type != ValueType.Float32)
                throw new StackTypeInvalidException(OpCode.Int32ReinterpretFloat32, ValueType.Float32, type);

            stack.Push(ValueType.Int32);

            context.Emit(OpCodes.Call, context[HelperMethod.Int32ReinterpretFloat32, (helper, c) =>
            {
                var builder = c.ExportsBuilder.DefineMethod(
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