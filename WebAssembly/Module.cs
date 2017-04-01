using System;
using System.Collections.Generic;
using System.IO;

namespace WebAssembly
{
	/// <summary>
	/// Contains raw information about a WebAssembly module.
	/// </summary>
	public class Module
	{
		/// <summary>
		/// Creates a new <see cref="Module"/> instance.
		/// </summary>
		public Module()
		{
		}

		/// <summary>
		/// Indicates that the source data is in the WebAssembly binary format.
		/// </summary>
		const uint magic = 0x6d736100;

		private IList<Section> sections;

		/// <summary>
		/// Sections within the module.
		/// </summary>
		public IList<Section> Sections
		{
			get => this.sections ?? (this.sections = new List<Section>());
			set => this.sections = value ?? throw new ArgumentNullException(nameof(value));
		}

		/// <summary>
		/// Creates a new <see cref="Module"/> from a stream.
		/// </summary>
		/// <param name="input">The source of data.  The stream is left open after reading is complete.</param>
		/// <returns>The module.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="input"/> cannot be null.</exception>
		/// <exception cref="ModuleLoadException">An error was encountered while reading the WebAssembly stream.</exception>
		public static Module From(Stream input)
		{
			using (var reader = new Reader(input))
			{
				try
				{
					if (reader.ReadUInt32() != magic)
						throw new ModuleLoadException("File preamble magic value is incorrect.", 0);

					switch (reader.ReadUInt32())
					{
						case 0x1: //First release
						case 0xd: //Final pre-release, binary format is identical with first release.
							break;
						default:
							throw new ModuleLoadException("Unsupported version, only version 0x1 and 0xd are accepted.", 4);
					}

					var module = new Module();

					while (reader.TryReadVarUInt7(out var id)) //At points where TryRead is used, the stream can safely end.
					{
						var payloadLength = reader.ReadVarUInt32();

						switch (id)
						{
							case 0:
								var preNameOffset = reader.Offset;
								var nameLength = reader.ReadVarUInt32();

								module.Sections.Add(new Sections.Custom
								{
									Name = reader.ReadString(nameLength),
									Content = reader.ReadBytes(payloadLength - checked((uint)(reader.Offset - preNameOffset))),
								});
								break;
							default:
								throw new ModuleLoadException($"Unrecognized section type {id}.", reader.Offset);
						}
					}

					return module;
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
		}
	}
}