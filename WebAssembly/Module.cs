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

		private IList<CustomSection> customSections;

		/// <summary>
		/// Custom sections.
		/// </summary>
		/// <exception cref="ArgumentNullException">Value cannot be set to null.</exception>
		public IList<CustomSection> CustomSections
		{
			get => this.customSections ?? (this.customSections = new List<CustomSection>());
			set => this.customSections = value ?? throw new ArgumentNullException(nameof(value));
		}

		private IList<Type> types;

		/// <summary>
		/// Function signatures.
		/// </summary>
		/// <exception cref="ArgumentNullException">Value cannot be set to null.</exception>
		public IList<Type> Types
		{
			get => this.types ?? (this.types = new List<Type>());
			set => this.types = value ?? throw new ArgumentNullException(nameof(value));
		}

		private IList<Import> imports;

		/// <summary>
		/// Imported external features.
		/// </summary>
		/// <exception cref="ArgumentNullException">Value cannot be set to null.</exception>
		public IList<Import> Imports
		{
			get => this.imports ?? (this.imports = new List<Import>());
			set => this.imports = value ?? throw new ArgumentNullException(nameof(value));
		}

		private IList<Function> functions;

		/// <summary>
		/// Functions defined within the assembly.
		/// </summary>
		/// <exception cref="ArgumentNullException">Value cannot be set to null.</exception>
		public IList<Function> Functions
		{
			get => this.functions ?? (this.functions = new List<Function>());
			set => this.functions = value ?? throw new ArgumentNullException(nameof(value));
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
							case 0: //Custom section
								{
									var preNameOffset = reader.Offset;
									var nameLength = reader.ReadVarUInt32();

									module.CustomSections.Add(new CustomSection
									{
										Name = reader.ReadString(nameLength),
										Content = reader.ReadBytes(payloadLength - checked((uint)(reader.Offset - preNameOffset))),
									});
								}
								break;

							case 1: //Function signature declarations
								{
									var count = reader.ReadVarUInt32();
									var types = module.types = new List<Type>(checked((int)count));

									for (var i = 0; i < count; i++)
										types.Add(new Type(reader));
								}
								break;

							case 2: //Import declarations
								{
									var count = reader.ReadVarUInt32();
									var imports = module.imports = new List<Import>(checked((int)count));

									for (var i = 0; i < count; i++)
										imports.Add(Import.ParseFrom(reader));
								}
								break;

							case 3: //Function declarations
								{
									var count = reader.ReadVarUInt32();
									var functions = module.functions = new List<Function>(checked((int)count));

									for (var i = 0; i < count; i++)
										functions.Add(new Function(reader.ReadVarUInt32()));
								}
								break;

							case 4: //Indirect function table and other tables

							case 5: //Memory attributes

							case 6: //Global declarations

							case 7: //Exports

							case 8: //Start function declaration

							case 9: //Elements section

							case 10: //Function bodies (code)

							case 11: //Data segments

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