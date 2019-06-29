using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Convert a signed 64-bit integer to a 32-bit float.
    /// </summary>
    public class Float32ConvertSignedInt64 : SimpleInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.Float32ConvertSignedInt64"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Float32ConvertSignedInt64;

        /// <summary>
        /// Creates a new  <see cref="Float32ConvertSignedInt64"/> instance.
        /// </summary>
        public Float32ConvertSignedInt64()
        {
        }

        internal sealed override void Compile(CompilationContext context)
        {
            var stack = context.Stack;
            if (stack.Count == 0)
                throw new StackTooSmallException(OpCode.Float32ConvertSignedInt64, 1, 0);

            var type = stack.Pop();
            if (type != ValueType.Int64)
                throw new StackTypeInvalidException(OpCode.Float32ConvertSignedInt64, ValueType.Int64, type);

            context.Emit(OpCodes.Conv_R4);

            stack.Push(ValueType.Float32);
        }
    }
}