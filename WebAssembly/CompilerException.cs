using System;
using System.Collections.Generic;
using System.Text;

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
	}
}