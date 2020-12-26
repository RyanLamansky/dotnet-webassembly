using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Extend a signed 32-bit integer to a 64-bit integer.
    /// </summary>
    public class Int64Extend32Signed : SimpleInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.Int64Extend32Signed"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Int64Extend32Signed;

        /// <summary>
        /// Creates a new  <see cref="Int64Extend32Signed"/> instance.
        /// </summary>
        public Int64Extend32Signed()
        {
        }

        internal sealed override void Compile(CompilationContext context)
        {
            var stack = context.Stack;

            context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int64);

            context.Emit(OpCodes.Conv_I4);
            context.Emit(OpCodes.Conv_I8);

            stack.Push(WebAssemblyValueType.Int64);
        }
    }
}