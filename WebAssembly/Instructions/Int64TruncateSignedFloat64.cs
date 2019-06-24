using System.Reflection.Emit;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Truncate a 64-bit float to a signed 64-bit integer.
    /// </summary>
    public class Int64TruncateSignedFloat64 : SimpleInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.Int64TruncateSignedFloat64"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Int64TruncateSignedFloat64;

        /// <summary>
        /// Creates a new  <see cref="Int64TruncateSignedFloat64"/> instance.
        /// </summary>
        public Int64TruncateSignedFloat64()
        {
        }

        internal sealed override void Compile(CompilationContext context)
        {
            var stack = context.Stack;
            if (stack.Count == 0)
                throw new StackTooSmallException(OpCode.Int64TruncateSignedFloat64, 1, 0);

            var type = stack.Pop();
            if (type != ValueType.Float64)
                throw new StackTypeInvalidException(OpCode.Int64TruncateSignedFloat64, ValueType.Float64, type);

            context.Emit(OpCodes.Conv_Ovf_I8);

            stack.Push(ValueType.Int64);
        }
    }
}