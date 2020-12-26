using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Identifies an instruction that uses a single CIL <see cref="System.Reflection.Emit.OpCode"/> to remove two values of the same type from the stack, returning a single <see cref="WebAssemblyValueType.Int32"/>.
    /// </summary>
    public abstract class ValueTwoToInt32Instruction : SimpleInstruction
    {
        private protected ValueTwoToInt32Instruction()
        {
        }

        private protected abstract WebAssemblyValueType ValueType { get; }

        private protected abstract System.Reflection.Emit.OpCode EmittedOpCode { get; }

        internal override void Compile(CompilationContext context)
        {
            var stack = context.Stack;

            context.PopStackNoReturn(this.OpCode, this.ValueType, this.ValueType);

            stack.Push(WebAssemblyValueType.Int32);

            context.Emit(this.EmittedOpCode);
        }
    }
}