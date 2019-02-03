namespace WebAssembly.Instructions
{
    /// <summary>
    /// Sign-agnostic multiplication (lower 32-bits).
    /// </summary>
    public class Int32Multiply : ValueTwoToOneInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.Int32Multiply"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Int32Multiply;

        private protected sealed override ValueType ValueType => ValueType.Int32;

        private protected sealed override System.Reflection.Emit.OpCode EmittedOpCode =>
            System.Reflection.Emit.OpCodes.Mul;

        /// <summary>
        /// Creates a new  <see cref="Int32Multiply"/> instance.
        /// </summary>
        public Int32Multiply()
        {
        }
    }
}