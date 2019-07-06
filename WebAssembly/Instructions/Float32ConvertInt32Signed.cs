using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Convert a signed 32-bit integer to a 32-bit float.
    /// </summary>
    public class Float32ConvertInt32Signed : SimpleInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.Float32ConvertInt32Signed"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Float32ConvertInt32Signed;

        /// <summary>
        /// Creates a new  <see cref="Float32ConvertInt32Signed"/> instance.
        /// </summary>
        public Float32ConvertInt32Signed()
        {
        }

        internal sealed override void Compile(CompilationContext context)
        {
            var stack = context.Stack;
            if (stack.Count == 0)
                throw new StackTooSmallException(OpCode.Float32ConvertInt32Signed, 1, 0);

            var type = stack.Pop();
            if (type != ValueType.Int32)
                throw new StackTypeInvalidException(OpCode.Float32ConvertInt32Signed, ValueType.Int32, type);

            context.Emit(OpCodes.Conv_R4);

            stack.Push(ValueType.Float32);
        }
    }
}