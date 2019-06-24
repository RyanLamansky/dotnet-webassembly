using System.Reflection.Emit;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Truncate a 64-bit float to a signed 32-bit integer.
    /// </summary>
    public class Int32TruncateSignedFloat64 : SimpleInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.Int32TruncateSignedFloat64"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Int32TruncateSignedFloat64;

        /// <summary>
        /// Creates a new  <see cref="Int32TruncateSignedFloat64"/> instance.
        /// </summary>
        public Int32TruncateSignedFloat64()
        {
        }

        internal sealed override void Compile(CompilationContext context)
        {
            var stack = context.Stack;
            if (stack.Count == 0)
                throw new StackTooSmallException(OpCode.Int32TruncateSignedFloat64, 1, 0);

            var type = stack.Pop();
            if (type != ValueType.Float64)
                throw new StackTypeInvalidException(OpCode.Int32TruncateSignedFloat64, ValueType.Float64, type);

            context.Emit(OpCodes.Conv_Ovf_I4);

            stack.Push(ValueType.Int32);
        }
    }
}