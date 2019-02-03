using System.Reflection.Emit;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Truncate a 32-bit float to a signed 64-bit integer.
    /// </summary>
    public class Int64TruncateSignedFloat32 : SimpleInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.Int64TruncateSignedFloat32"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Int64TruncateSignedFloat32;

        /// <summary>
        /// Creates a new  <see cref="Int64TruncateSignedFloat32"/> instance.
        /// </summary>
        public Int64TruncateSignedFloat32()
        {
        }

        internal sealed override void Compile(CompilationContext context)
        {
            var stack = context.Stack;
            if (stack.Count == 0)
                throw new StackTooSmallException(OpCode.Int64TruncateSignedFloat32, 1, 0);

            var type = stack.Pop();
            if (type != ValueType.Float32)
                throw new StackTypeInvalidException(OpCode.Int64TruncateSignedFloat32, ValueType.Float32, type);

            context.Emit(OpCodes.Conv_Ovf_I8);

            stack.Push(ValueType.Int64);
        }
    }
}