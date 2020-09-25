using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Extend a signed 8-bit integer to a 64-bit integer.
    /// </summary>
    public class Int64Extend8Signed : SimpleInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.Int64Extend8Signed"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Int64Extend8Signed;

        /// <summary>
        /// Creates a new  <see cref="Int64Extend8Signed"/> instance.
        /// </summary>
        public Int64Extend8Signed()
        {
        }

        internal sealed override void Compile(CompilationContext context)
        {
            var stack = context.Stack;
            if (stack.Count < 1)
                throw new StackTooSmallException(this.OpCode, 1, stack.Count);

            var type = stack.Pop();

            if (type != WebAssemblyValueType.Int64)
                throw new StackTypeInvalidException(this.OpCode, WebAssemblyValueType.Int64, type);

            context.Emit(OpCodes.Conv_I1);
            context.Emit(OpCodes.Conv_I8);

            stack.Push(WebAssemblyValueType.Int64);
        }
    }
}