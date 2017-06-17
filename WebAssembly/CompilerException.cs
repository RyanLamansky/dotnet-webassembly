using System;

namespace WebAssembly
{
	/// <summary>
	/// Describes an error that occurred during the compilation process.
	/// </summary>
	public class CompilerException : Exception
	{
		/// <summary>
		/// Creates a new <see cref="CompilerException"/> instance with the provided message.
		/// </summary>
		/// <param name="message">Becomes <see cref="Exception.Message"/>.</param>
		public CompilerException(string message)
			: base(message)
		{
		}

		/// <summary>
		/// Creates a new <see cref="CompilerException"/> instance with the provided message and inner exception.
		/// </summary>
		/// <param name="message">Becomes <see cref="Exception.Message"/>.</param>
		/// <param name="innerException">Becomes <see cref="Exception.InnerException"/>.</param>
		public CompilerException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}