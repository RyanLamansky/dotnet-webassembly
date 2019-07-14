using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Convert an unsigned 32-bit integer to a 64-bit float.
    /// </summary>
    public class Float64ConvertInt32Unsigned : SimpleInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.Float64ConvertInt32Unsigned"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Float64ConvertInt32Unsigned;

        /// <summary>
        /// Creates a new  <see cref="Float64ConvertInt32Unsigned"/> instance.
        /// </summary>
        public Float64ConvertInt32Unsigned()
        {
        }

        internal sealed override void Compile(CompilationContext context)
        {
            var stack = context.Stack;
            if (stack.Count == 0)
                throw new StackTooSmallException(OpCode.Float64ConvertInt32Unsigned, 1, 0);

            var type = stack.Pop();
            if (type != WebAssemblyValueType.Int32)
                throw new StackTypeInvalidException(OpCode.Float64ConvertInt32Unsigned, WebAssemblyValueType.Int32, type);

            context.Emit(OpCodes.Conv_R_Un);
            context.Emit(OpCodes.Conv_R8);

            stack.Push(WebAssemblyValueType.Float64);
        }
    }
}