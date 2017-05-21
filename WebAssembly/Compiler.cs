using System;
using System.IO;
using System.Reflection;

namespace WebAssembly
{
	using Compiled;

	/// <summary>
	/// Provides compilation functionality.  Use <see cref="Module"/> for robust inspection and modification capability.
	/// </summary>
	public static class Compiler
	{
		/// <summary>
		/// Uses streaming compilation to create an executable <see cref="Instance{TExports}"/> from a binary WebAssembly source.
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
		public static Func<Instance<TExports>> FromBinary<TExports>(string path)
		where TExports : class
		{
			using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4 * 1024, FileOptions.SequentialScan))
			{
				return FromBinary<TExports>(stream);
			}
		}

		/// <summary>
		/// Uses streaming compilation to create an executable <see cref="Instance{TExports}"/> from a binary WebAssembly source.
		/// </summary>
		/// <param name="input">The source of data.  The stream is left open after reading is complete.</param>
		/// <returns>A function that creates instances on demand.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="input"/> cannot be null.</exception>
		public static Func<Instance<TExports>> FromBinary<TExports>(Stream input)
		where TExports : class
		{
			var exportInfo = typeof(TExports).GetTypeInfo();
			if (!exportInfo.IsPublic && !exportInfo.IsNestedPublic)
				throw new CompilerException($"Export type {exportInfo.FullName} must be public so that the compiler can inherit it.");

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

			var constructor = parsed.Compile(typeof(Instance<TExports>), typeof(TExports));

			return () => (Instance<TExports>)constructor.Invoke(null);
		}
	}
}