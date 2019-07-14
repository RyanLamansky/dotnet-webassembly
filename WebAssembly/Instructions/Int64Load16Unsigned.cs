using System.Reflection.Emit;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Load 2 bytes and zero-extend i16 to i64.
    /// </summary>
    public class Int64Load16Unsigned : MemoryReadInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.Int64Load16Unsigned"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Int64Load16Unsigned;

        /// <summary>
        /// Creates a new  <see cref="Int64Load16Unsigned"/> instance.
        /// </summary>
        public Int64Load16Unsigned()
        {
        }

        internal Int64Load16Unsigned(Reader reader)
            : base(reader)
        {
        }

        private protected sealed override WebAssemblyValueType Type => WebAssemblyValueType.Int64;

        private protected sealed override byte Size => 2;

        private protected sealed override System.Reflection.Emit.OpCode EmittedOpCode => OpCodes.Ldind_U2;

        private protected sealed override System.Reflection.Emit.OpCode ConversionOpCode => OpCodes.Conv_U8;
    }
}