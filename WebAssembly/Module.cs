using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using WebAssembly.Runtime;

namespace WebAssembly
{
    /// <summary>
    /// Contains raw information about a WebAssembly module.  Use <see cref="Compile"/> if you wish to execute a WebAssembly file.
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
        internal const uint Magic = 0x6d736100;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] //Wrapped by a property
        private IList<CustomSection>? customSections;

        /// <summary>
        /// Custom sections.
        /// </summary>
        /// <exception cref="ArgumentNullException">Value cannot be set to null.</exception>
        public IList<CustomSection> CustomSections
        {
            get => this.customSections ??= new List<CustomSection>();
            set => this.customSections = value ?? throw new ArgumentNullException(nameof(value));
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] //Wrapped by a property
        private IList<WebAssemblyType>? types;

        /// <summary>
        /// Function signatures.
        /// </summary>
        /// <exception cref="ArgumentNullException">Value cannot be set to null.</exception>
        public IList<WebAssemblyType> Types
        {
            get => this.types ??= new List<WebAssemblyType>();
            set => this.types = value ?? throw new ArgumentNullException(nameof(value));
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] //Wrapped by a property
        private IList<Import>? imports;

        /// <summary>
        /// Imported external features.
        /// </summary>
        /// <exception cref="ArgumentNullException">Value cannot be set to null.</exception>
        public IList<Import> Imports
        {
            get => this.imports ??= new List<Import>();
            set => this.imports = value ?? throw new ArgumentNullException(nameof(value));
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] //Wrapped by a property
        private IList<Function>? functions;

        /// <summary>
        /// Functions defined within the assembly.
        /// </summary>
        /// <exception cref="ArgumentNullException">Value cannot be set to null.</exception>
        public IList<Function> Functions
        {
            get => this.functions ??= new List<Function>();
            set => this.functions = value ?? throw new ArgumentNullException(nameof(value));
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] //Wrapped by a property
        private IList<Table>? tables;

        /// <summary>
        /// Tables defined within the assembly.
        /// </summary>
        /// <exception cref="ArgumentNullException">Value cannot be set to null.</exception>
        public IList<Table> Tables
        {
            get => this.tables ??= new List<Table>();
            set => this.tables = value ?? throw new ArgumentNullException(nameof(value));
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] //Wrapped by a property
        private IList<Memory>? memories;

        /// <summary>
        /// Linear memory areas defined within the assembly.
        /// </summary>
        /// <exception cref="ArgumentNullException">Value cannot be set to null.</exception>
        public IList<Memory> Memories
        {
            get => this.memories ??= new List<Memory>();
            set => this.memories = value ?? throw new ArgumentNullException(nameof(value));
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] //Wrapped by a property
        private IList<Global>? globals;

        /// <summary>
        /// Global values defined within the assembly.
        /// </summary>
        /// <exception cref="ArgumentNullException">Value cannot be set to null.</exception>
        public IList<Global> Globals
        {
            get => this.globals ??= new List<Global>();
            set => this.globals = value ?? throw new ArgumentNullException(nameof(value));
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] //Wrapped by a property
        private IList<Export>? exports;

        /// <summary>
        /// Features to be made available to the host environment.
        /// </summary>
        /// <exception cref="ArgumentNullException">Value cannot be set to null.</exception>
        public IList<Export> Exports
        {
            get => this.exports ??= new List<Export>();
            set => this.exports = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Gets or sets the start function index, or null if no start function is present.
        /// </summary>
        public uint? Start { get; set; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] //Wrapped by a property
        private IList<Element>? elements;

        /// <summary>
        /// The elements section allows a module to initialize (at instantiation time) the elements of any imported or internally-defined table with any other definition in the module
        /// </summary>
        /// <exception cref="ArgumentNullException">Value cannot be set to null.</exception>
        public IList<Element> Elements
        {
            get => this.elements ??= new List<Element>();
            set => this.elements = value ?? throw new ArgumentNullException(nameof(value));
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] //Wrapped by a property
        private IList<FunctionBody>? codes;

        /// <summary>
        /// The code section contains a body for every function in the module.
        /// </summary>
        /// <exception cref="ArgumentNullException">Value cannot be set to null.</exception>
        public IList<FunctionBody> Codes
        {
            get => this.codes ??= new List<FunctionBody>();
            set => this.codes = value ?? throw new ArgumentNullException(nameof(value));
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] //Wrapped by a property
        private IList<Data>? data;

        /// <summary>
        /// The data section declares the initialized data that is loaded into the linear memory.
        /// </summary>
        /// <exception cref="ArgumentNullException">Value cannot be set to null.</exception>
        public IList<Data> Data
        {
            get => this.data ??= new List<Data>();
            set => this.data = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Creates a new <see cref="Module"/> from a file.
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
        public static Module ReadFromBinary(string path)
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4 * 1024, FileOptions.SequentialScan);
            return ReadFromBinary(stream);
        }

        /// <summary>
        /// Creates a new <see cref="Module"/> from a stream.
        /// </summary>
        /// <param name="input">The source of data.  The stream is left open after reading is complete.</param>
        /// <returns>The module.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="input"/> cannot be null.</exception>
        /// <exception cref="ModuleLoadException">An error was encountered while reading the WebAssembly stream.</exception>
        public static Module ReadFromBinary(Stream input)
        {
            using var reader = new Reader(input);
            try
            {
                if (reader.ReadUInt32() != Magic)
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
                var previousSection = Section.None;
                var preSectionOffset = reader.Offset;
                while (reader.TryReadVarUInt7(out var id)) //At points where TryRead is used, the stream can safely end.
                {
                    if (id != 0 && (Section)id < previousSection)
                        throw new ModuleLoadException($"Sections out of order; section {(Section)id} encounterd after {previousSection}.", preSectionOffset);
                    var payloadLength = reader.ReadVarUInt32();

                    switch ((Section)id)
                    {
                        case Section.None: //Custom section
                            {
                                var preNameOffset = reader.Offset;
                                var nameLength = reader.ReadVarUInt32();

                                module.CustomSections.Add(new CustomSection
                                {
                                    Name = reader.ReadString(nameLength),
                                    Content = reader.ReadBytes(payloadLength - checked((uint)(reader.Offset - preNameOffset))),
                                    PrecedingSection = previousSection
                                });
                            }
                            continue; //Skip normal post-section logic since this isn't a real section.

                        case Section.Type: //Function signature declarations
                            {
                                var count = reader.ReadVarUInt32();
                                var types = module.types = new List<WebAssemblyType>(checked((int)count));

                                for (var i = 0; i < count; i++)
                                    types.Add(new WebAssemblyType(reader));
                            }
                            break;

                        case Section.Import: //Import declarations
                            {
                                var count = reader.ReadVarUInt32();
                                var imports = module.imports = new List<Import>(checked((int)count));

                                for (var i = 0; i < count; i++)
                                    imports.Add(Import.ParseFrom(reader));
                            }
                            break;

                        case Section.Function: //Function declarations
                            {
                                var count = reader.ReadVarUInt32();
                                var functions = module.functions = new List<Function>(checked((int)count));

                                for (var i = 0; i < count; i++)
                                    functions.Add(new Function(reader.ReadVarUInt32()));
                            }
                            break;

                        case Section.Table: //Indirect function table and other tables
                            {
                                var count = reader.ReadVarUInt32();
                                var tables = module.tables = new List<Table>(checked((int)count));

                                for (var i = 0; i < count; i++)
                                    tables.Add(new Table(reader));
                            }
                            break;

                        case Section.Memory: //Memory attributes
                            {
                                var count = reader.ReadVarUInt32();
                                var memories = module.memories = new List<Memory>(checked((int)count));

                                for (var i = 0; i < count; i++)
                                    memories.Add(new Memory(reader));
                            }
                            break;

                        case Section.Global: //Global declarations
                            {
                                var count = reader.ReadVarUInt32();
                                var globals = module.globals = new List<Global>(checked((int)count));

                                for (var i = 0; i < count; i++)
                                    globals.Add(new Global(reader));
                            }
                            break;

                        case Section.Export: //Exports
                            {
                                var count = reader.ReadVarUInt32();
                                var exports = module.exports = new List<Export>(checked((int)count));

                                for (var i = 0; i < count; i++)
                                    exports.Add(new Export(reader));
                            }
                            break;

                        case Section.Start: //Start function declaration
                            module.Start = reader.ReadVarUInt32();
                            break;

                        case Section.Element: //Elements section
                            {
                                var count = reader.ReadVarUInt32();
                                var elements = module.elements = new List<Element>(checked((int)count));

                                for (var i = 0; i < count; i++)
                                    elements.Add(new Element(reader));
                            }
                            break;

                        case Section.Code: //Function bodies (code)
                            {
                                var count = reader.ReadVarUInt32();
                                var codes = module.codes = new List<FunctionBody>(checked((int)count));

                                for (var i = 0; i < count; i++)
                                    codes.Add(new FunctionBody(reader, reader.ReadVarUInt32()));
                            }
                            break;

                        case Section.Data: //Data segments
                            {
                                var count = reader.ReadVarUInt32();
                                var data = module.data = new List<Data>(checked((int)count));

                                for (var i = 0; i < count; i++)
                                    data.Add(new Data(reader));
                            }
                            break;

                        default:
                            throw new ModuleLoadException($"Unrecognized section type {id}.", preSectionOffset);
                    }

                    previousSection = (Section)id;
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

        /// <summary>
        /// Writes the contents of this module to a <see cref="Stream"/>.
        /// </summary>
        /// <param name="output">The destination for data.  The stream is left open after reading is complete.</param>
        /// <exception cref="ArgumentNullException"><paramref name="output"/> cannot be null.</exception>
        public void WriteToBinary(Stream output)
        {
            static bool LastOpCodeIsNotEnd(IList<Instruction> instruction)
            {
                return
                    instruction == null ||
                    instruction.Count == 0 ||
                    instruction[instruction.Count - 1].OpCode != OpCode.End
                    ;
            };

            if (this.globals != null)
            {
                var index = 0;
                foreach (var global in this.globals)
                {
                    if (LastOpCodeIsNotEnd(global.InitializerExpression))
                        throw new InvalidOperationException($"Global at index {index} has an initializer expression not terminated with OpCode.End.");

                    index++;
                }
            }

            if (this.elements != null)
            {
                var index = 0;
                foreach (var element in this.elements)
                {
                    if (LastOpCodeIsNotEnd(element.InitializerExpression))
                        throw new InvalidOperationException($"Element at index {index} has an initializer expression not terminated with OpCode.End.");

                    index++;
                }
            }

            if (this.codes != null)
            {
                var index = 0;
                foreach (var code in this.codes)
                {
                    if (LastOpCodeIsNotEnd(code.Code))
                        throw new InvalidOperationException($"Code at index {index} has a body not terminated with OpCode.End.");

                    index++;
                }
            }

            if (this.data != null)
            {
                var index = 0;
                foreach (var data in this.data)
                {
                    if (LastOpCodeIsNotEnd(data.InitializerExpression))
                        throw new InvalidOperationException($"Data at index {index} has an initializer expression not terminated with OpCode.End.");

                    index++;
                }
            }

            var customSectionsByPrecedingSection = this.customSections?
                .Where(custom => custom != null)
                .GroupBy(custom => custom.PrecedingSection)
                .ToDictionary(group => group.Key);

            using var writer = new Writer(output);
            writer.Write(Magic);
            writer.Write((uint)0x1);

            var buffer = new byte[4 * 1024];
            WriteCustomSection(buffer, writer, Section.None, customSectionsByPrecedingSection);

            if (this.types != null)
            {
                WriteSection(buffer, writer, Section.Type, sectionWriter =>
                {
                    sectionWriter.WriteVar((uint)this.types.Count);
                    foreach (var type in this.types)
                        type?.WriteTo(sectionWriter);
                });
            }
            WriteCustomSection(buffer, writer, Section.Type, customSectionsByPrecedingSection);

            if (this.imports != null)
            {
                WriteSection(buffer, writer, Section.Import, sectionWriter =>
                {
                    sectionWriter.WriteVar((uint)this.imports.Count);
                    foreach (var import in this.imports)
                        import?.WriteTo(sectionWriter);
                });
            }
            WriteCustomSection(buffer, writer, Section.Import, customSectionsByPrecedingSection);

            if (this.functions != null)
            {
                WriteSection(buffer, writer, Section.Function, sectionWriter =>
                {
                    sectionWriter.WriteVar((uint)this.functions.Count);
                    foreach (var function in this.functions)
                        function?.WriteTo(sectionWriter);
                });
            }
            WriteCustomSection(buffer, writer, Section.Function, customSectionsByPrecedingSection);

            if (this.tables != null)
            {
                WriteSection(buffer, writer, Section.Table, sectionWriter =>
                {
                    sectionWriter.WriteVar((uint)this.tables.Count);
                    foreach (var table in this.tables)
                        table?.WriteTo(sectionWriter);
                });
            }
            WriteCustomSection(buffer, writer, Section.Table, customSectionsByPrecedingSection);

            if (this.memories != null)
            {
                WriteSection(buffer, writer, Section.Memory, sectionWriter =>
                {
                    sectionWriter.WriteVar((uint)this.memories.Count);
                    foreach (var memory in this.memories)
                        memory?.WriteTo(sectionWriter);
                });
            }
            WriteCustomSection(buffer, writer, Section.Memory, customSectionsByPrecedingSection);

            if (this.globals != null)
            {
                WriteSection(buffer, writer, Section.Global, sectionWriter =>
                {
                    sectionWriter.WriteVar((uint)this.globals.Count);
                    foreach (var global in this.globals)
                        global?.WriteTo(sectionWriter);
                });
            }
            WriteCustomSection(buffer, writer, Section.Global, customSectionsByPrecedingSection);

            if (this.exports != null)
            {
                WriteSection(buffer, writer, Section.Export, sectionWriter =>
                {
                    sectionWriter.WriteVar((uint)this.exports.Count);
                    foreach (var export in this.exports)
                        export?.WriteTo(sectionWriter);
                });
            }
            WriteCustomSection(buffer, writer, Section.Export, customSectionsByPrecedingSection);

            if (this.Start != null)
            {
                WriteSection(buffer, writer, Section.Start, sectionWriter =>
                {
                    sectionWriter.WriteVar(this.Start.GetValueOrDefault());
                });
            }
            WriteCustomSection(buffer, writer, Section.Start, customSectionsByPrecedingSection);

            if (this.elements != null)
            {
                WriteSection(buffer, writer, Section.Element, sectionWriter =>
                {
                    sectionWriter.WriteVar((uint)this.elements.Count);
                    foreach (var element in this.elements)
                        element?.WriteTo(sectionWriter);
                });
            }
            WriteCustomSection(buffer, writer, Section.Element, customSectionsByPrecedingSection);

            if (this.codes != null)
            {
                WriteSection(buffer, writer, Section.Code, sectionWriter =>
                {
                    sectionWriter.WriteVar((uint)this.codes.Count);
                    foreach (var code in this.codes)
                        code?.WriteTo(sectionWriter, buffer);
                });
            }
            WriteCustomSection(buffer, writer, Section.Code, customSectionsByPrecedingSection);

            if (this.data != null)
            {
                WriteSection(buffer, writer, Section.Data, sectionWriter =>
                {
                    sectionWriter.WriteVar((uint)this.data.Count);
                    foreach (var data in this.data)
                        data?.WriteTo(sectionWriter);
                });
            }
            WriteCustomSection(buffer, writer, Section.Data, customSectionsByPrecedingSection);
        }

        static void WriteCustomSection(
            byte[] buffer,
            Writer writer,
            Section precedingSection,
            Dictionary<Section, IGrouping<Section, CustomSection>>? customSectionsByPrecedingSection
            )
        {
            if (customSectionsByPrecedingSection == null)
                return;
            if (!customSectionsByPrecedingSection.TryGetValue(precedingSection, out var entries))
                return;

            foreach (var custom in entries)
            {
                WriteSection(buffer, writer, Section.None, sectionWriter =>
                {
                    custom.WriteTo(sectionWriter);
                });
            }
        }

        static void WriteSection(byte[] buffer, Writer writer, Section section, Action<Writer> action)
        {
            writer.Write((byte)section);
            using var memory = new MemoryStream();
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

        /// <summary>
        /// Creates an executable <see cref="Instance{TExports}"/> from this instance's data.
        /// This is intended for use with run-time code generation.  For directly compiling WebAssembly byte code, use <see cref="Runtime.Compile"/>.
        /// </summary>
		/// <returns>A function that creates runnable instances.</returns>
		/// <exception cref="ModuleLoadException">An error was encountered while reading the WebAssembly file.</exception>
        public InstanceCreator<TExports> Compile<TExports>()
        where TExports : class
        {
            //TODO: A more direct compilation process will be faster and create less garbage.
            using var memory = new MemoryStream();
            this.WriteToBinary(memory);
            memory.Position = 0;
            return Runtime.Compile.FromBinary<TExports>(memory);
        }
    }
}