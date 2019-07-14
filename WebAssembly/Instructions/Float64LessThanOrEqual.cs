namespace WebAssembly.Instructions
{
    /// <summary>
    /// Compare ordered and less than or equal.
    /// </summary>
    public class Float64LessThanOrEqual : ValueTwoToInt32NotEqualZeroInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.Float64LessThanOrEqual"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Float64LessThanOrEqual;

        private protected sealed override WebAssemblyValueType ValueType => WebAssemblyValueType.Float64;

        private protected sealed override System.Reflection.Emit.OpCode EmittedOpCode =>
            System.Reflection.Emit.OpCodes.Cgt_Un; //The result is compared for equality to zero, reversing it.

        /// <summary>
        /// Creates a new  <see cref="Float64LessThanOrEqual"/> instance.
        /// </summary>
        public Float64LessThanOrEqual()
        {
        }
    }
}