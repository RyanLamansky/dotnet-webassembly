namespace WebAssembly.Instructions
{
    /// <summary>
    /// Signed less than or equal.
    /// </summary>
    public class Int64LessThanOrEqualSigned : ValueTwoToInt32NotEqualZeroInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.Int64LessThanOrEqualSigned"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Int64LessThanOrEqualSigned;

        private protected sealed override WebAssemblyValueType ValueType => WebAssemblyValueType.Int64;

        private protected sealed override System.Reflection.Emit.OpCode EmittedOpCode =>
            System.Reflection.Emit.OpCodes.Cgt; //The result is compared for equality to zero, reversing it.

        /// <summary>
        /// Creates a new  <see cref="Int64LessThanOrEqualSigned"/> instance.
        /// </summary>
        public Int64LessThanOrEqualSigned()
        {
        }
    }
}