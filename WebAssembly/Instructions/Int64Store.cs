using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// (No conversion) store 8 bytes.
    /// </summary>
    public class Int64Store : MemoryWriteInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.Int64Store"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Int64Store;

        /// <summary>
        /// Creates a new  <see cref="Int64Store"/> instance.
        /// </summary>
        public Int64Store()
        {
        }

        internal Int64Store(Reader reader)
            : base(reader)
        {
        }

        private protected sealed override ValueType Type => ValueType.Int64;

        private protected sealed override byte Size => 8;

        private protected sealed override System.Reflection.Emit.OpCode EmittedOpCode => OpCodes.Stind_I8;

        private protected sealed override HelperMethod StoreHelper => HelperMethod.StoreInt64FromInt64;
    }
}