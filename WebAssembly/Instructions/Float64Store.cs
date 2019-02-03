using System.Reflection.Emit;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// (No conversion) store 8 bytes.
    /// </summary>
    public class Float64Store : MemoryWriteInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.Float64Store"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Float64Store;

        /// <summary>
        /// Creates a new  <see cref="Float64Store"/> instance.
        /// </summary>
        public Float64Store()
        {
        }

        internal Float64Store(Reader reader)
            : base(reader)
        {
        }

        private protected sealed override ValueType Type => ValueType.Float64;

        private protected sealed override byte Size => 8;

        private protected sealed override System.Reflection.Emit.OpCode EmittedOpCode => OpCodes.Stind_R8;

        private protected sealed override HelperMethod StoreHelper => HelperMethod.StoreFloat64;
    }
}