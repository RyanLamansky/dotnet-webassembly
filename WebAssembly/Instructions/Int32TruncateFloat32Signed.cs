using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Truncate a 32-bit float to a signed 32-bit integer.
    /// </summary>
    public class Int32TruncateFloat32Signed : SimpleInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.Int32TruncateFloat32Signed"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Int32TruncateFloat32Signed;

        /// <summary>
        /// Creates a new  <see cref="Int32TruncateFloat32Signed"/> instance.
        /// </summary>
        public Int32TruncateFloat32Signed()
        {
        }

        internal sealed override void Compile(CompilationContext context)
        {
            var stack = context.Stack;

            context.PopStackNoReturn(OpCode.Int32TruncateFloat32Signed, WebAssemblyValueType.Float32);

            context.Emit(OpCodes.Conv_Ovf_I4);

            stack.Push(WebAssemblyValueType.Int32);
        }
    }
}