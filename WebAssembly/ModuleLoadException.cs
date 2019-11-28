using System;

namespace WebAssembly
{
    /// <summary>
    /// Represents errors that occur during the loading process of a <see cref="Module"/>.
    /// </summary>
    public class ModuleLoadException : Exception
    {
        /// <summary>
        /// Creates a new <see cref="ModuleLoadException"/> with the provided parameters.
        /// </summary>
        /// <param name="message">Becomes the <see cref="Exception.Message"/> value.</param>
        /// <param name="offset">The offset from the start of a data sequence where the error was encountered.</param>
        /// <param name="innerException">The wrapped exception.</param>
        public ModuleLoadException(string message, long offset, Exception? innerException)
            : base($"At offset {offset}: {message}", innerException)
        {
            this.Offset = offset;
        }

        /// <summary>
        /// Creates a new <see cref="ModuleLoadException"/> with the provided parameters.
        /// </summary>
        /// <param name="message">Becomes the <see cref="Exception.Message"/> value.</param>
        /// <param name="offset">The offset from the start of a data sequence where the error was encountered.</param>
        public ModuleLoadException(string message, long offset)
            : this(message, offset, null)
        {
            this.Offset = offset;
        }

        /// <summary>
        /// The offset within the source that triggered the load failure.
        /// </summary>
        public long Offset { get; }
    }
}