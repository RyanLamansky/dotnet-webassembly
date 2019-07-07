namespace WebAssembly
{
    /// <summary>
    /// Describes a function.
    /// </summary>
    public class Function
    {
        /// <summary>
        /// The index to the <see cref="Module.Types"/> entry that describes the function signature.
        /// </summary>
        public uint Type { get; set; }

        /// <summary>
        /// Creates a new <see cref="Function"/> instance.
        /// </summary>
        public Function()
        {
        }

        /// <summary>
        /// Creates a new <see cref="Function"/> instance with the provided type index.
        /// </summary>
        /// <param name="type">The index to the <see cref="Module.Types"/> entry that describes the function signature.</param>
        public Function(uint type)
        {
            this.Type = type;
        }

        /// <summary>
        /// Expresses the value of this instance as a string.
        /// </summary>
        /// <returns>A string representation of this instance.</returns>
        public override string ToString() => $"Function {Type}";

        internal void WriteTo(Writer writer)
        {
            writer.WriteVar(this.Type);
        }
    }
}