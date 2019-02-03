namespace WebAssembly.Instructions
{
    /// <summary>
    /// Compare unordered or unequal.
    /// </summary>
    public class Float64NotEqual : ValueTwoToInt32NotEqualZeroInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.Float64NotEqual"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Float64NotEqual;

        private protected sealed override ValueType ValueType => ValueType.Float64;

        private protected sealed override System.Reflection.Emit.OpCode EmittedOpCode =>
            System.Reflection.Emit.OpCodes.Ceq; //The result is compared for equality to zero, reversing it.

        /// <summary>
        /// Creates a new  <see cref="Float64NotEqual"/> instance.
        /// </summary>
        public Float64NotEqual()
        {
        }
    }
}