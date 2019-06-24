using System.Reflection.Emit;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Wrap a 64-bit integer to a 32-bit integer.
    /// </summary>
    public class Int32WrapInt64 : SimpleInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.Int32WrapInt64"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Int32WrapInt64;

        /// <summary>
        /// Creates a new  <see cref="Int32WrapInt64"/> instance.
        /// </summary>
        public Int32WrapInt64()
        {
        }

        internal sealed override void Compile(CompilationContext context)
        {
            var stack = context.Stack;
            if (stack.Count < 1)
                throw new StackTooSmallException(this.OpCode, 1, stack.Count);

            var type = stack.Pop();

            if (type != ValueType.Int64)
                throw new StackTypeInvalidException(this.OpCode, ValueType.Int64, type);

            context.Emit(OpCodes.Conv_I4);

            stack.Push(ValueType.Int32);
        }
    }
}