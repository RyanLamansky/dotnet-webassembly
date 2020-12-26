using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Extend a signed 16-bit integer to a 64-bit integer.
    /// </summary>
    public class Int64Extend16Signed : SimpleInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.Int64Extend16Signed"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Int64Extend16Signed;

        /// <summary>
        /// Creates a new  <see cref="Int64Extend16Signed"/> instance.
        /// </summary>
        public Int64Extend16Signed()
        {
        }

        internal sealed override void Compile(CompilationContext context)
        {
            var stack = context.Stack;

            context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int64);

            context.Emit(OpCodes.Conv_I2);
            context.Emit(OpCodes.Conv_I8);

            stack.Push(WebAssemblyValueType.Int64);
        }
    }
}