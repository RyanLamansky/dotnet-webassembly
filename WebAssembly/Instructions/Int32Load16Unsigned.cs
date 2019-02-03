using System.Reflection.Emit;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Load 2 bytes and zero-extend i16 to i32.
    /// </summary>
    public class Int32Load16Unsigned : MemoryReadInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.Int32Load16Unsigned"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Int32Load16Unsigned;

        /// <summary>
        /// Creates a new  <see cref="Int32Load16Unsigned"/> instance.
        /// </summary>
        public Int32Load16Unsigned()
        {
        }

        internal Int32Load16Unsigned(Reader reader)
            : base(reader)
        {
        }

        private protected sealed override ValueType Type => ValueType.Int32;

        private protected sealed override byte Size => 2;

        private protected sealed override System.Reflection.Emit.OpCode EmittedOpCode => OpCodes.Ldind_U2;
    }
}