using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Compare equal to zero (return 1 if operand is zero, 0 otherwise).
    /// </summary>
    public class Int32EqualZero : SimpleInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.Int32EqualZero"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Int32EqualZero;

        /// <summary>
        /// Creates a new  <see cref="Int32EqualZero"/> instance.
        /// </summary>
        public Int32EqualZero()
        {
        }

        internal sealed override void Compile(CompilationContext context)
        {
            //Assuming validation passes, the remaining type will be Int32.
            context.ValidateStack(this.OpCode, WebAssemblyValueType.Int32);

            context.Emit(OpCodes.Ldc_I4_0);
            context.Emit(OpCodes.Ceq);
        }
    }
}