using System.Reflection.Emit;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// The beginning of an if construct with an implicit "then" block.
    /// </summary>
    public class If : BlockTypeInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.If"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.If;

        /// <summary>
        /// Creates a new  <see cref="If"/> instance.
        /// </summary>
        public If()
        {
        }

        /// <summary>
        /// Creates a new <see cref="If"/> of the provided type.
        /// </summary>
        /// <param name="type">Becomes the block's <see cref="BlockTypeInstruction.Type"/>.</param>
        public If(BlockType type)
            : base(type)
        {
        }

        internal If(Reader reader)
            : base(reader)
        {
        }

        internal sealed override void Compile(CompilationContext context)
        {
            var stack = context.Stack;
            if (stack.Count == 0)
                throw new StackTooSmallException(OpCode.If, 1, 0);

            var type = stack.Pop();
            if (type != ValueType.Int32)
                throw new StackTypeInvalidException(OpCode.If, ValueType.Int32, type);

            var label = context.DefineLabel();
            context.Labels.Add(checked((uint)context.Depth.Count), label);
            context.Depth.Push(Type);
            context.Emit(OpCodes.Brfalse, label);
        }
    }
}