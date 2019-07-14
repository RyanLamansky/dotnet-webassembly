namespace WebAssembly.Instructions
{
    /// <summary>
    /// Sign-agnostic compare unequal.
    /// </summary>
    public class Int64NotEqual : ValueTwoToInt32NotEqualZeroInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.Int64NotEqual"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Int64NotEqual;

        private protected sealed override WebAssemblyValueType ValueType => WebAssemblyValueType.Int64;

        private protected sealed override System.Reflection.Emit.OpCode EmittedOpCode =>
            System.Reflection.Emit.OpCodes.Ceq; //The result is compared for equality to zero, reversing it.

        /// <summary>
        /// Creates a new  <see cref="Int64NotEqual"/> instance.
        /// </summary>
        public Int64NotEqual()
        {
        }
    }
}