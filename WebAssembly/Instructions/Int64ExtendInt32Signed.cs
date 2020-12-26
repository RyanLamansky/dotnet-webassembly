using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Extend a signed 32-bit integer to a 64-bit integer.
    /// </summary>
    public class Int64ExtendInt32Signed : SimpleInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.Int64ExtendInt32Signed"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Int64ExtendInt32Signed;

        /// <summary>
        /// Creates a new  <see cref="Int64ExtendInt32Signed"/> instance.
        /// </summary>
        public Int64ExtendInt32Signed()
        {
        }

        internal sealed override void Compile(CompilationContext context)
        {
            var stack = context.Stack;

            context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int32);

            context.Emit(OpCodes.Conv_I8);

            stack.Push(WebAssemblyValueType.Int64);
        }
    }
}