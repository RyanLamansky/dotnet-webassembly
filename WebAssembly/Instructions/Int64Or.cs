namespace WebAssembly.Instructions
{
    /// <summary>
    /// Sign-agnostic bitwise inclusive or.
    /// </summary>
    public class Int64Or : ValueTwoToOneInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.Int64Or"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Int64Or;

        private protected sealed override WebAssemblyValueType ValueType => WebAssemblyValueType.Int64;

        private protected sealed override System.Reflection.Emit.OpCode EmittedOpCode =>
            System.Reflection.Emit.OpCodes.Or;

        /// <summary>
        /// Creates a new  <see cref="Int64Or"/> instance.
        /// </summary>
        public Int64Or()
        {
        }
    }
}