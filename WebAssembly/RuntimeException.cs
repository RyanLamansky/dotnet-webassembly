using System;

namespace WebAssembly
{
    /// <summary>
    /// Describes an error encountered during execution of a compiled WebAssembly.
    /// </summary>
    public class RuntimeException : Exception
    {
        /// <summary>
        /// Creates a new <see cref="RuntimeException"/> with the provided message.
        /// </summary>
        /// <param name="message">Becomes <see cref="Exception.Message"/>.</param>
        public RuntimeException(string message)
            : base(message)
        {
        }
    }
}