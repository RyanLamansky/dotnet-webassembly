using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Identifies an instruction that uses a single CIL <see cref="System.Reflection.Emit.OpCode"/> to remove two values from the stack, replacing it with one value, all of a specific type.
    /// </summary>
    public abstract class ValueTwoToOneInstruction : SimpleInstruction
    {
        private protected ValueTwoToOneInstruction()
        {
        }

        private protected abstract WebAssemblyValueType ValueType { get; }

        private protected abstract System.Reflection.Emit.OpCode EmittedOpCode { get; }

        internal sealed override void Compile(CompilationContext context)
        {
            var stack = context.Stack;
            
            context.PopStackNoReturn(this.OpCode, this.ValueType, this.ValueType);
            stack.Push(this.ValueType);

            context.Emit(this.EmittedOpCode);
        }
    }
}