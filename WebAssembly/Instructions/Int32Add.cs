namespace WebAssembly.Instructions
{
    /// <summary>
    /// Sign-agnostic addition.
    /// </summary>
    public class Int32Add : ValueTwoToOneInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.Int32Add"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Int32Add;

        private protected sealed override WebAssemblyValueType ValueType => WebAssemblyValueType.Int32;

        private protected sealed override System.Reflection.Emit.OpCode EmittedOpCode =>
            System.Reflection.Emit.OpCodes.Add;

        /// <summary>
        /// Creates a new  <see cref="Int32Add"/> instance.
        /// </summary>
        public Int32Add()
        {
        }
    }
}