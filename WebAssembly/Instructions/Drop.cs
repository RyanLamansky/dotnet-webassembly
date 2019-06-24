using System.Reflection.Emit;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// A unary operator that discards the value of its operand.
    /// </summary>
    public class Drop : SimpleInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.Drop"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Drop;

        /// <summary>
        /// Creates a new  <see cref="Drop"/> instance.
        /// </summary>
        public Drop()
        {
        }

        internal sealed override void Compile(CompilationContext context)
        {
            var stack = context.Stack;
            if (stack.Count == 0)
                throw new StackTooSmallException(OpCode.Drop, 1, 0);

            stack.Pop();

            context.Emit(OpCodes.Pop);
        }
    }
}