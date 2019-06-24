using System.Reflection.Emit;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Wrap i64 to i8 and store 1 byte.
    /// </summary>
    public class Int64Store8 : MemoryWriteInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.Int64Store8"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Int64Store8;

        /// <summary>
        /// Creates a new  <see cref="Int64Store8"/> instance.
        /// </summary>
        public Int64Store8()
        {
        }

        internal Int64Store8(Reader reader)
            : base(reader)
        {
        }

        private protected sealed override ValueType Type => ValueType.Int64;

        private protected sealed override byte Size => 1;

        private protected sealed override System.Reflection.Emit.OpCode EmittedOpCode => OpCodes.Stind_I1;

        private protected sealed override HelperMethod StoreHelper => HelperMethod.StoreInt8FromInt64;
    }
}