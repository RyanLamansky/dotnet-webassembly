using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Truncate a 32-bit float to a signed 64-bit integer.
    /// </summary>
    public class Int64TruncateFloat32Signed : SimpleInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.Int64TruncateFloat32Signed"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Int64TruncateFloat32Signed;

        /// <summary>
        /// Creates a new  <see cref="Int64TruncateFloat32Signed"/> instance.
        /// </summary>
        public Int64TruncateFloat32Signed()
        {
        }

        internal sealed override void Compile(CompilationContext context)
        {
            var stack = context.Stack;

            context.PopStackNoReturn(OpCode.Int64TruncateFloat32Signed, WebAssemblyValueType.Float32);

            context.Emit(OpCodes.Conv_Ovf_I8);

            stack.Push(WebAssemblyValueType.Int64);
        }
    }
}