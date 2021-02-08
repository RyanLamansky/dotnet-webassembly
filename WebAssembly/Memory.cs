using System;
using System.Diagnostics;

namespace WebAssembly
{
    /// <summary>
    /// Desribes a linear memory area within the assembly.
    /// </summary>
    public class Memory
    {
        /// <summary>
        /// The standard memory page size.
        /// </summary>
        public const uint PageSize = 65536;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] //Wrapped by a property
        private ResizableLimits? resizableLimits;

        /// <summary>
        /// A packed tuple that describes the limits of the memory.
        /// </summary>
        public ResizableLimits ResizableLimits
        {
            get => resizableLimits ??= new ResizableLimits();
            set => resizableLimits = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Creates a new <see cref="Memory"/> instance.
        /// </summary>
        public Memory()
        {
        }

        /// <summary>
        /// Creates a new <see cref="Memory"/> instance with the provided <see cref="ResizableLimits.Minimum"/> and <see cref="ResizableLimits.Maximum"/> values.
        /// </summary>
        /// <param name="minimum">Initial length (in units of table elements or 65,536-byte pages).</param>
        /// <param name="maximum">Maximum length (in units of table elements or 65,536-byte pages).</param>
        public Memory(uint minimum, uint? maximum)
        {
            this.resizableLimits = new ResizableLimits(minimum, maximum);
        }

        /// <summary>
        /// Creates a new <see cref="Memory"/> from a binary data stream.
        /// </summary>
        /// <param name="reader">The source of data.</param>
        /// <exception cref="ArgumentNullException"><paramref name="reader"/> cannot be null.</exception>
        internal Memory(Reader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            this.resizableLimits = new ResizableLimits(reader);
        }

        /// <summary>
        /// Expresses the value of this instance as a string.
        /// </summary>
        /// <returns>A string representation of this instance.</returns>
        public override string ToString() => $"Memory {ResizableLimits}";

        internal void WriteTo(Writer writer)
        {
            this.ResizableLimits.WriteTo(writer);
        }
    }
}