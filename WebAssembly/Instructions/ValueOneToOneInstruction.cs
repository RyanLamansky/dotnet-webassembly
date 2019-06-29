using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Identifies an instruction that uses a single CIL <see cref="System.Reflection.Emit.OpCode"/> to remove one value from the stack, replacing it with one value, both of a specific type.
    /// </summary>
    public abstract class ValueOneToOneInstruction : SimpleInstruction
    {
        private protected ValueOneToOneInstruction()
        {
        }

        private protected abstract ValueType ValueType { get; }

        private protected abstract System.Reflection.Emit.OpCode EmittedOpCode { get; }

        internal sealed override void Compile(CompilationContext context)
        {
            var stack = context.Stack;
            if (stack.Count < 1)
                throw new StackTooSmallException(this.OpCode, 1, stack.Count);

            var type = stack.Peek(); //Assuming validation passes, the remaining type will be this.

            if (type != this.ValueType)
                throw new StackTypeInvalidException(this.OpCode, this.ValueType, type);

            context.Emit(this.EmittedOpCode);
        }
    }
}