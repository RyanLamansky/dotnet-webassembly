using System;
using System.IO;

namespace WebAssembly
{
	using Compiled;

	/// <summary>
	/// Provides compilation functionality.  Use <see cref="Module"/> for robust inspection and modification capability.
	/// </summary>
	public static class Compiler
	{
		/// <summary>
		/// Uses streaming compilation to create an executable <see cref="Instance"/> from a binary WebAssembly source.
		/// </summary>
		/// <param name="path">The path to the file that contains a WebAssembly binary stream.</param>
		/// <returns>The module.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="path"/> cannot be null.</exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="path"/> is an empty string (""), contains only white space, or contains one or more invalid characters; or,
		/// <paramref name="path"/> refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in an NTFS environment.
		/// </exception>
		/// <exception cref="NotSupportedException"><paramref name="path"/> refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in a non-NTFS environment.</exception>
		/// <exception cref="FileNotFoundException">The file indicated by <paramref name="path"/> could not be found.</exception>
		/// <exception cref="DirectoryNotFoundException">The specified <paramref name="path"/> is invalid, such as being on an unmapped drive.</exception>
		/// <exception cref="PathTooLongException">
		/// The specified path, file name, or both exceed the system-defined maximum length.
		/// For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters.</exception>
		/// <exception cref="ModuleLoadException">An error was encountered while reading the WebAssembly file.</exception>
		public static Func<Instance> FromBinary(string path)
		{
			using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4 * 1024, FileOptions.SequentialScan))
			{
				return FromBinary(stream);
			}
		}

		/// <summary>
		/// Uses streaming compilation to create an executable <see cref="Instance"/> from a binary WebAssembly source.
		/// </summary>
		/// <param name="input">The source of data.  The stream is left open after reading is complete.</param>
		/// <returns></returns>
		public static Func<Instance> FromBinary(Stream input)
		{
			Parsed parsed;
			using (var reader = new Reader(input))
			{
				try
				{
					parsed = new Parsed(reader);
				}
				catch (OverflowException x)
				{
					throw new ModuleLoadException("Overflow encountered.", reader.Offset, x);
				}
				catch (EndOfStreamException x)
				{
					throw new ModuleLoadException("Stream ended unexpectedly.", reader.Offset, x);
				}
				catch (Exception x)
				{
					throw new ModuleLoadException(x.Message, reader.Offset, x);
				}
			}

			return parsed.Compile();
		}
	}
}