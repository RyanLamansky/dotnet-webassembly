namespace WebAssembly.Instructions
{
    /// <summary>
    /// Unsigned division (result is floored).
    /// </summary>
    public class Int32DivideUnsigned : ValueTwoToOneInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.Int32DivideUnsigned"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Int32DivideUnsigned;

        private protected sealed override WebAssemblyValueType ValueType => WebAssemblyValueType.Int32;

        private protected sealed override System.Reflection.Emit.OpCode EmittedOpCode =>
            System.Reflection.Emit.OpCodes.Div_Un;

        /// <summary>
        /// Creates a new  <see cref="Int32DivideUnsigned"/> instance.
        /// </summary>
        public Int32DivideUnsigned()
        {
        }
    }
}