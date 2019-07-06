using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Convert an unsigned 32-bit integer to a 32-bit float.
    /// </summary>
    public class Float32ConvertInt32Unsigned : SimpleInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.Float32ConvertInt32Unsigned"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Float32ConvertInt32Unsigned;

        /// <summary>
        /// Creates a new  <see cref="Float32ConvertInt32Unsigned"/> instance.
        /// </summary>
        public Float32ConvertInt32Unsigned()
        {
        }

        internal sealed override void Compile(CompilationContext context)
        {
            var stack = context.Stack;
            if (stack.Count == 0)
                throw new StackTooSmallException(OpCode.Float32ConvertInt32Unsigned, 1, 0);

            var type = stack.Pop();
            if (type != ValueType.Int32)
                throw new StackTypeInvalidException(OpCode.Float32ConvertInt32Unsigned, ValueType.Int32, type);

            context.Emit(OpCodes.Conv_R_Un);
            context.Emit(OpCodes.Conv_R4);

            stack.Push(ValueType.Float32);
        }
    }
}