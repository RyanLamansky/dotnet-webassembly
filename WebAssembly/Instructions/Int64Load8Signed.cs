using System.Reflection.Emit;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Load 1 byte and sign-extend i8 to i64.
    /// </summary>
    public class Int64Load8Signed : MemoryReadInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.Int64Load8Signed"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Int64Load8Signed;

        /// <summary>
        /// Creates a new  <see cref="Int64Load8Signed"/> instance.
        /// </summary>
        public Int64Load8Signed()
        {
        }

        internal Int64Load8Signed(Reader reader)
            : base(reader)
        {
        }

        private protected sealed override WebAssemblyValueType Type => WebAssemblyValueType.Int64;

        private protected sealed override byte Size => 1;

        private protected sealed override System.Reflection.Emit.OpCode EmittedOpCode => OpCodes.Ldind_I1;

        private protected sealed override System.Reflection.Emit.OpCode ConversionOpCode => OpCodes.Conv_I8;
    }
}