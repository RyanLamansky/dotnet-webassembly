using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

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

            context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int64);

            context.Emit(OpCodes.Conv_I4);

            stack.Push(WebAssemblyValueType.Int32);
        }
    }
}