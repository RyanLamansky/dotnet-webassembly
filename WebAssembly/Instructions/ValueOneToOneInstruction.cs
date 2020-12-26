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

        private protected abstract WebAssemblyValueType ValueType { get; }

        private protected abstract System.Reflection.Emit.OpCode EmittedOpCode { get; }

        internal sealed override void Compile(CompilationContext context)
        {
            //Assuming validation passes, the remaining type will be this.ValueType.
            context.ValidateStack(this.OpCode, this.ValueType);

            context.Emit(this.EmittedOpCode);
        }
    }
}