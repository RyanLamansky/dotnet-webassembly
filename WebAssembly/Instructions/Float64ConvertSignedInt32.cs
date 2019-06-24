using System.Reflection.Emit;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Convert a signed 32-bit integer to a 64-bit float.
    /// </summary>
    public class Float64ConvertSignedInt32 : SimpleInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.Float64ConvertSignedInt32"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Float64ConvertSignedInt32;

        /// <summary>
        /// Creates a new  <see cref="Float64ConvertSignedInt32"/> instance.
        /// </summary>
        public Float64ConvertSignedInt32()
        {
        }

        internal sealed override void Compile(CompilationContext context)
        {
            var stack = context.Stack;
            if (stack.Count == 0)
                throw new StackTooSmallException(OpCode.Float64ConvertSignedInt32, 1, 0);

            var type = stack.Pop();
            if (type != ValueType.Int32)
                throw new StackTypeInvalidException(OpCode.Float64ConvertSignedInt32, ValueType.Int32, type);

            context.Emit(OpCodes.Conv_R8);

            stack.Push(ValueType.Float64);
        }
    }
}