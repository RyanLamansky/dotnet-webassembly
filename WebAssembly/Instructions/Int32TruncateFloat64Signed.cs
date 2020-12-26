using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Truncate a 64-bit float to a signed 32-bit integer.
    /// </summary>
    public class Int32TruncateFloat64Signed : SimpleInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.Int32TruncateFloat64Signed"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Int32TruncateFloat64Signed;

        /// <summary>
        /// Creates a new  <see cref="Int32TruncateFloat64Signed"/> instance.
        /// </summary>
        public Int32TruncateFloat64Signed()
        {
        }

        internal sealed override void Compile(CompilationContext context)
        {
            var stack = context.Stack;

            context.PopStackNoReturn(OpCode.Int32TruncateFloat64Signed, WebAssemblyValueType.Float64);

            context.Emit(OpCodes.Conv_Ovf_I4);

            stack.Push(WebAssemblyValueType.Int32);
        }
    }
}