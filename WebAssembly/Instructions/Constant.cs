using System;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Base class for the constant instructions <see cref="Int32Constant"/>, <see cref="Int64Constant"/>, <see cref="Float32Constant"/>, and <see cref="Float64Constant"/>.
    /// </summary>
    /// <typeparam name="T">The type of constant, one of <see cref="int"/>, <see cref="long"/>, <see cref="float"/>, or <see cref="double"/>.</typeparam>
    public abstract class Constant<T> : Instruction, IEquatable<Constant<T>>
        where T : struct, IComparable, IComparable<T>, IConvertible, IEquatable<T>, IFormattable
    {
        /// <summary>
        /// Gets or sets the value of the constant.
        /// </summary>
        public T Value { get; set; }

        private protected Constant()
        {
        }

        private protected Constant(T value) => Value = value;

        /// <summary>
        /// Returns a simple hash code based on <see cref="Value"/> and the <see cref="OpCode"/> of the instruction.
        /// </summary>
        /// <returns>The hash code.</returns>
        /// <remarks><see cref="Value"/> should not be changed while this instance is used as a hash key.</remarks>
        public override int GetHashCode() => HashCode.Combine((int)this.OpCode, this.Value.GetHashCode());

        /// <summary>
        /// Determines whether this instruction is identical to another.
        /// </summary>
        /// <param name="other">The instruction to compare against.</param>
        /// <returns>True if they have the same type and value, otherwise false.</returns>
        public override bool Equals(Instruction? other) => other is Constant<T> instruction && this.Equals(instruction);

        /// <summary>
        /// Determines whether this instruction is identical to another.
        /// </summary>
        /// <param name="other">The instruction to compare against.</param>
        /// <returns>True if they have the same type and value, otherwise false.</returns>
        public bool Equals(Constant<T>? other) => (other?.Value.Equals(this.Value)).GetValueOrDefault();

        /// <summary>
        /// Provides a native representation of the instruction.
        /// </summary>
        /// <returns>A string representation of this instance.</returns>
        public override string ToString() => $"{base.ToString()} {Value}";
    }
}
