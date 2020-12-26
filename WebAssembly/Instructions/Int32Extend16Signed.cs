using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Extend a signed 16-bit integer to a 32-bit integer.
    /// </summary>
    public class Int32Extend16Signed : SimpleInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.Int32Extend16Signed"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Int32Extend16Signed;

        /// <summary>
        /// Creates a new  <see cref="Int32Extend16Signed"/> instance.
        /// </summary>
        public Int32Extend16Signed()
        {
        }

        internal sealed override void Compile(CompilationContext context)
        {
            var stack = context.Stack;

            context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int32);

            context.Emit(OpCodes.Conv_I2);
            context.Emit(OpCodes.Conv_I4);

            stack.Push(WebAssemblyValueType.Int32);
        }
    }
}