using System;
using System.Collections.Generic;
using System.IO;
using static System.Diagnostics.Debug;

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

		private IList<Table> tables;

		/// <summary>
		/// Tables defined within the assembly.
		/// </summary>
		/// <exception cref="ArgumentNullException">Value cannot be set to null.</exception>
		public IList<Table> Tables
		{
			get => this.tables ?? (this.tables = new List<Table>());
			set => this.tables = value ?? throw new ArgumentNullException(nameof(value));
		}

		private IList<Memory> memories;

		/// <summary>
		/// Linear memory areas defined within the assembly.
		/// </summary>
		/// <exception cref="ArgumentNullException">Value cannot be set to null.</exception>
		public IList<Memory> Memories
		{
			get => this.memories ?? (this.memories = new List<Memory>());
			set => this.memories = value ?? throw new ArgumentNullException(nameof(value));
		}

		private IList<Global> globals;

		/// <summary>
		/// Global values defined within the assembly.
		/// </summary>
		/// <exception cref="ArgumentNullException">Value cannot be set to null.</exception>
		public IList<Global> Globals
		{
			get => this.globals ?? (this.globals = new List<Global>());
			set => this.globals = value ?? throw new ArgumentNullException(nameof(value));
		}

		private IList<Export> exports;

		/// <summary>
		/// Features to be made available to the host environment.
		/// </summary>
		/// <exception cref="ArgumentNullException">Value cannot be set to null.</exception>
		public IList<Export> Exports
		{
			get => this.exports ?? (this.exports = new List<Export>());
			set => this.exports = value ?? throw new ArgumentNullException(nameof(value));
		}

		/// <summary>
		/// Gets or sets the start function index, or null if no start function is present.
		/// </summary>
		public uint? Start { get; set; }

		private IList<Element> elements;

		/// <summary>
		/// The elements section allows a module to initialize (at instantiation time) the elements of any imported or internally-defined table with any other definition in the module
		/// </summary>
		/// <exception cref="ArgumentNullException">Value cannot be set to null.</exception>
		public IList<Element> Elements
		{
			get => this.elements ?? (this.elements = new List<Element>());
			set => this.elements = value ?? throw new ArgumentNullException(nameof(value));
		}

		private IList<FunctionBody> codes;

		/// <summary>
		/// The code section contains a body for every function in the module.
		/// </summary>
		/// <exception cref="ArgumentNullException">Value cannot be set to null.</exception>
		public IList<FunctionBody> Codes
		{
			get => this.codes ?? (this.codes = new List<FunctionBody>());
			set => this.codes = value ?? throw new ArgumentNullException(nameof(value));
		}

		private IList<Data> data;

		/// <summary>
		/// The data section declares the initialized data that is loaded into the linear memory.
		/// </summary>
		/// <exception cref="ArgumentNullException">Value cannot be set to null.</exception>
		public IList<Data> Data
		{
			get => this.data ?? (this.data = new List<Data>());
			set => this.data = value ?? throw new ArgumentNullException(nameof(value));
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
								{
									var count = reader.ReadVarUInt32();
									var tables = module.tables = new List<Table>(checked((int)count));

									for (var i = 0; i < count; i++)
										tables.Add(new Table(reader));
								}
								break;

							case 5: //Memory attributes
								{
									var count = reader.ReadVarUInt32();
									var memories = module.memories = new List<Memory>(checked((int)count));

									for (var i = 0; i < count; i++)
										memories.Add(new Memory(reader));
								}
								break;

							case 6: //Global declarations
								{
									var count = reader.ReadVarUInt32();
									var globals = module.globals = new List<Global>(checked((int)count));

									for (var i = 0; i < count; i++)
										globals.Add(new Global(reader));
								}
								break;

							case 7: //Exports
								{
									var count = reader.ReadVarUInt32();
									var exports= module.exports = new List<Export>(checked((int)count));

									for (var i = 0; i < count; i++)
										exports.Add(new Export(reader));
								}
								break;

							case 8: //Start function declaration
								module.Start = reader.ReadVarUInt32();
								break;

							case 9: //Elements section
								{
									var count = reader.ReadVarUInt32();
									var elements = module.elements = new List<Element>(checked((int)count));

									for (var i = 0; i < count; i++)
										elements.Add(new Element(reader));
								}
								break;

							case 10: //Function bodies (code)
								{
									var count = reader.ReadVarUInt32();
									var codes = module.codes = new List<FunctionBody>(checked((int)count));

									for (var i = 0; i < count; i++)
										codes.Add(new FunctionBody(reader, reader.ReadVarUInt32()));
								}
								break;

							case 11: //Data segments
								{
									var count = reader.ReadVarUInt32();
									var data = module.data = new List<Data>(checked((int)count));

									for (var i = 0; i < count; i++)
										data.Add(new Data(reader));
								}
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

		/// <summary>
		/// Writes the contents of this module to a <see cref="Stream"/>.
		/// </summary>
		/// <param name="output">The destination for data.  The stream is left open after reading is complete.</param>
		/// <exception cref="ArgumentNullException"><paramref name="output"/> cannot be null.</exception>
		public void ToBinary(Stream output)
		{
			using (var writer = new Writer(output))
			{
				writer.Write(magic);
				writer.Write(0x1);

				var buffer = new Byte[4 * 1024];

				if (this.types != null)
				{
					WriteSection(buffer, writer, 1, sectionWriter =>
					{
						sectionWriter.WriteVar((uint)this.types.Count);
						foreach (var type in this.types)
							type?.WriteTo(writer);
					});
				}

				if (this.imports != null)
				{
				}

				if (this.functions != null)
				{
				}

				if (this.tables != null)
				{
				}

				if (this.memories != null)
				{
				}

				if (this.globals != null)
				{
				}

				if (this.exports != null)
				{
				}

				if (this.Start != null)
				{
				}

				if (this.elements != null)
				{
				}

				if (this.codes != null)
				{
				}

				if (this.data != null)
				{
				}
			}
		}

		static void WriteSection(Byte[] buffer, Writer writer, byte section, Action<Writer> action)
		{
			Assert(buffer != null);
			Assert(writer != null);
			Assert(action != null);

			writer.Write(section);
			using (var memory = new MemoryStream())
			{
				using (var sectionWriter = new Writer(memory))
				{
					action(sectionWriter);
				}

				writer.WriteVar(checked((uint)memory.Length));
				memory.Position = 0;
				int read;
				while ((read = memory.Read(buffer, 0, buffer.Length)) > 0)
					writer.Write(buffer, 0, read);
			}
		}
	}
}