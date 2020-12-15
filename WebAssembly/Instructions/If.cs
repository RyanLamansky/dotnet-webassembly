using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

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
            if (type != WebAssemblyValueType.Int32)
                throw new StackTypeInvalidException(OpCode.If, WebAssemblyValueType.Int32, type);

            var label = context.DefineLabel();
            context.Labels.Add(checked((uint)context.Depth.Count), label);
            context.Depth.Push(Type);
            context.BlockContexts.Add(checked((uint)context.Depth.Count), new BlockContext(context.Stack));
            context.Emit(OpCodes.Brfalse, label);
        }
    }
}