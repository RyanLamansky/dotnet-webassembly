using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// (i32 index, T value){T} => i32 index; Write a global variable.
    /// </summary>
    public class GlobalSet : VariableAccessInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.GlobalSet"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.GlobalSet;

        /// <summary>
        /// Creates a new  <see cref="GlobalSet"/> instance.
        /// </summary>
        public GlobalSet()
        {
        }

        /// <summary>
        /// Creates a new <see cref="GlobalSet"/> for the provided variable index.
        /// </summary>
        /// <param name="index">The index of the variable to access.</param>
        public GlobalSet(uint index)
            : base(index)
        {
        }

        internal GlobalSet(Reader reader)
            : base(reader)
        {
        }

        internal sealed override void Compile(CompilationContext context)
        {
            if (context.Globals == null)
                throw new CompilerException("Can't use SetGlobal without a global section or global imports.");

            var stack = context.Stack;
            if (stack.Count < 1)
                throw new StackTooSmallException(OpCode.GlobalSet, 1, stack.Count);

            GlobalInfo global;
            try
            {
                global = context.Globals[this.Index];
            }
            catch (System.IndexOutOfRangeException x)
            {
                throw new CompilerException($"Global at index {this.Index} does not exist.", x);
            }

            if (global.Setter == null)
                throw new CompilerException($"Global at index {this.Index} is immutable.");

            var type = stack.Pop();
            if (type != global.Type)
                throw new StackTypeInvalidException(OpCode.GlobalSet, global.Type, type);

            if (global.RequiresInstance)
                context.EmitLoadThis();

            context.Emit(OpCodes.Call, global.Setter);
        }
    }
}