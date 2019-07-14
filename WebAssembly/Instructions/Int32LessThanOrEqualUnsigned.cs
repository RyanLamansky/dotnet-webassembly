namespace WebAssembly.Instructions
{
    /// <summary>
    /// Unsigned less than or equal.
    /// </summary>
    public class Int32LessThanOrEqualUnsigned : ValueTwoToInt32NotEqualZeroInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.Int32LessThanOrEqualUnsigned"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Int32LessThanOrEqualUnsigned;

        private protected sealed override WebAssemblyValueType ValueType => WebAssemblyValueType.Int32;

        private protected sealed override System.Reflection.Emit.OpCode EmittedOpCode =>
            System.Reflection.Emit.OpCodes.Cgt_Un; //The result is compared for equality to zero, reversing it.

        /// <summary>
        /// Creates a new  <see cref="Int32LessThanOrEqualUnsigned"/> instance.
        /// </summary>
        public Int32LessThanOrEqualUnsigned()
        {
        }
    }
}