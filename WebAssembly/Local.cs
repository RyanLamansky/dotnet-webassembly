using System;

namespace WebAssembly
{
    /// <summary>
    /// Each local entry declares a number of local variables of a given type.
    /// </summary>
    public class Local : IEquatable<Local>
    {
        /// <summary>
        ///  The number of local variables of <see cref="Type"/>.
        /// </summary>
        public uint Count { get; set; }

        /// <summary>
        /// Type of the variables.
        /// </summary>
        public WebAssemblyValueType Type { get; set; }

        /// <summary>
        /// Creates a new <see cref="Local"/> instance.
        /// </summary>
        public Local()
        {
        }

        internal Local(Reader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            this.Count = reader.ReadVarUInt32();
            this.Type = (WebAssemblyValueType)reader.ReadVarInt7();
        }

        /// <summary>
        /// Expresses the value of this instance as a string.
        /// </summary>
        /// <returns>A string representation of this instance.</returns>
        public override string ToString() => $"{Count} of {Type}";

        /// <summary>
        /// Returns a hash code based on the value of this instance.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode() => (int)this.Type | (int)this.Count << 8;

        /// <summary>
        /// Determines whether this instance is identical to another.
        /// </summary>
        /// <param name="obj">The object instance to compare against.</param>
        /// <returns>True if they have the same type and value, otherwise false.</returns>
        public override bool Equals(object? obj) => this.Equals(obj as Local);

        /// <summary>
        /// Determines whether this instance is identical to another.
        /// </summary>
        /// <param name="other">The instance to compare against.</param>
        /// <returns>True if they have the same type and value, otherwise false.</returns>
        public bool Equals(Local? other)
        {
            return other != null
                && other.Type == this.Type
                && other.Count == this.Count;
        }

        internal void WriteTo(Writer writer)
        {
            writer.WriteVar(this.Count);
            writer.WriteVar((sbyte)this.Type);
        }
    }
}