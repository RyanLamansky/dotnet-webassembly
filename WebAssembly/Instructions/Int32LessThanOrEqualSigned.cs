namespace WebAssembly.Instructions
{
    /// <summary>
    /// Signed less than or equal.
    /// </summary>
    public class Int32LessThanOrEqualSigned : ValueTwoToInt32NotEqualZeroInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.Int32LessThanOrEqualSigned"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Int32LessThanOrEqualSigned;

        private protected sealed override WebAssemblyValueType ValueType => WebAssemblyValueType.Int32;

        private protected sealed override System.Reflection.Emit.OpCode EmittedOpCode =>
            System.Reflection.Emit.OpCodes.Cgt; //The result is compared for equality to zero, reversing it.

        /// <summary>
        /// Creates a new  <see cref="Int32LessThanOrEqualSigned"/> instance.
        /// </summary>
        public Int32LessThanOrEqualSigned()
        {
        }
    }
}