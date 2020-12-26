using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Convert a signed 32-bit integer to a 32-bit float.
    /// </summary>
    public class Float32ConvertInt32Signed : SimpleInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.Float32ConvertInt32Signed"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Float32ConvertInt32Signed;

        /// <summary>
        /// Creates a new  <see cref="Float32ConvertInt32Signed"/> instance.
        /// </summary>
        public Float32ConvertInt32Signed()
        {
        }

        internal sealed override void Compile(CompilationContext context)
        {
            var stack = context.Stack;
            
            context.PopStackNoReturn(OpCode.Float32ConvertInt32Signed, WebAssemblyValueType.Int32);

            context.Emit(OpCodes.Conv_R4);

            stack.Push(WebAssemblyValueType.Float32);
        }
    }
}