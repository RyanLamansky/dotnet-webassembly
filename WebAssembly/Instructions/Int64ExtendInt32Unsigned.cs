using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Extend an unsigned 32-bit integer to a 64-bit integer.
    /// </summary>
    public class Int64ExtendInt32Unsigned : SimpleInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.Int64ExtendInt32Unsigned"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Int64ExtendInt32Unsigned;

        /// <summary>
        /// Creates a new  <see cref="Int64ExtendInt32Unsigned"/> instance.
        /// </summary>
        public Int64ExtendInt32Unsigned()
        {
        }

        internal sealed override void Compile(CompilationContext context)
        {
            var stack = context.Stack;

            context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int32);

            context.Emit(OpCodes.Conv_U8);

            stack.Push(WebAssemblyValueType.Int64);
        }
    }
}