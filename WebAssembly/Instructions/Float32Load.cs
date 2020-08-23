using System.Reflection.Emit;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Load 4 bytes as f32.
    /// </summary>
    public class Float32Load : MemoryReadInstruction, System.IEquatable<Float32Load>
    {
        /// <summary>
        /// Always <see cref="OpCode.Float32Load"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Float32Load;

        /// <summary>
        /// Creates a new  <see cref="Float32Load"/> instance.
        /// </summary>
        public Float32Load()
        {
        }

        internal Float32Load(Reader reader)
            : base(reader)
        {
        }

        private protected sealed override WebAssemblyValueType Type => WebAssemblyValueType.Float32;

        private protected sealed override byte Size => 4;

        private protected sealed override System.Reflection.Emit.OpCode EmittedOpCode => OpCodes.Ldind_R4;

        /// <summary>
        /// Determines whether this instruction is identical to another.
        /// </summary>
        /// <param name="other">The instruction to compare against.</param>
        /// <returns>True if they have the same type and value, otherwise false.</returns>
        public bool Equals(Float32Load? other) => base.Equals(other);
    }
}