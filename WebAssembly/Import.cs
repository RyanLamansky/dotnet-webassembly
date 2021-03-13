using System;
using System.Diagnostics;

namespace WebAssembly
{
    /// <summary>
    /// Describes external features made available to the web assembly.
    /// </summary>
    public abstract class Import
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] //Wrapped by a property
        private string? module;

        /// <summary>
        /// The module portion of the name.
        /// </summary>
        /// <exception cref="ArgumentNullException">Value cannot be set to null.</exception>
        public string Module
        {
            get => module ?? "";
            set => module = value ?? throw new ArgumentNullException(nameof(value));
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] //Wrapped by a property
        private string? field;

        /// <summary>
        /// The field portion of the name.
        /// </summary>
        /// <exception cref="ArgumentNullException">Value cannot be set to null.</exception>
        public string Field
        {
            get => field ?? "";
            set => field = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// The type of import.
        /// </summary>
        public abstract ExternalKind Kind { get; }

        /// <summary>
        /// Creates a new <see cref="Import"/> instance.
        /// </summary>
        private protected Import()
        {
        }

        private protected Import(string module, string field)
        {
            this.module = module;
            this.field = field;
        }

        /// <summary>
        /// Expresses the value of this instance as a string.
        /// </summary>
        /// <returns>A string representation of this instance.</returns>
        public override string ToString() => $"{Module}::{Field}";

        internal virtual void WriteTo(Writer writer)
        {
            writer.Write(this.Module);
            writer.Write(this.Field);
        }

        /// <summary>
        /// Creates an <see cref="Import"/> instance from the provided <see cref="Reader"/>.
        /// </summary>
        /// <param name="reader">Provides raw data.</param>
        /// <returns>The parsed <see cref="Import"/>.</returns>
        /// <exception cref="ModuleLoadException">Imported external kind is not recognized or not supported.</exception>
        internal static Import ParseFrom(Reader reader)
        {
            var module = reader.ReadString(reader.ReadVarUInt32());
            var field = reader.ReadString(reader.ReadVarUInt32());
            var initialOffset = reader.Offset;
            var kind = (ExternalKind)reader.ReadByte();

            return kind switch
            {
                ExternalKind.Function => new Function
                {
                    Module = module,
                    Field = field,
                    TypeIndex = reader.ReadVarUInt32(),
                },
                ExternalKind.Table => new Table
                {
                    Module = module,
                    Field = field,
                    Definition = new WebAssembly.Table(reader),
                },
                ExternalKind.Memory => new Memory
                {
                    Module = module,
                    Field = field,
                    Type = new WebAssembly.Memory(reader),
                },
                ExternalKind.Global => new Global(reader)
                {
                    Module = module,
                    Field = field,
                },
                _ => throw new ModuleLoadException($"Imported external kind of {kind} is not recognized.", initialOffset),
            };
        }

        /// <summary>
        /// Describes an imported function.
        /// </summary>
        public class Function : Import
        {
            /// <summary>
            /// Always <see cref="ExternalKind.Function"/>.
            /// </summary>
            public sealed override ExternalKind Kind => ExternalKind.Function;

            /// <summary>
            /// The index within the module's types that describes the function signature.
            /// </summary>
            public uint TypeIndex { get; set; }

            /// <summary>
            /// Creates a new <see cref="Function"/> instance.
            /// </summary>
            public Function()
            {
            }

            /// <summary>
            /// Creates a new <see cref="Function"/> instance with the provided parameters.
            /// </summary>
            /// <param name="module">The module portion of the name.</param>
            /// <param name="field">The field portion of the name.</param>
            /// <param name="typeIndex">The index within the module's types that describes the function signature.</param>
            public Function(string module, string field, uint typeIndex = 0)
                : base(module, field)
            {
                this.TypeIndex = typeIndex;
            }

            /// <summary>
            /// Expresses the value of this instance as a string.
            /// </summary>
            /// <returns>A string representation of this instance.</returns>
            public override string ToString() => $"{base.ToString()} ({nameof(Function)} {TypeIndex}";

            internal sealed override void WriteTo(Writer writer)
            {
                base.WriteTo(writer);
                writer.Write((byte)ExternalKind.Function);
                writer.WriteVar(this.TypeIndex);
            }
        }

        /// <summary>
        /// Describes an imported table.
        /// </summary>
        public class Table : Import
        {
            /// <summary>
            /// Always <see cref="ExternalKind.Table"/>.
            /// </summary>
            public sealed override ExternalKind Kind => ExternalKind.Table;

            /// <summary>
            /// Definiton of the imported table.
            /// </summary>
            public WebAssembly.Table? Definition { get; set; }

            /// <summary>
            /// Creates a new <see cref="Table"/> instance.
            /// </summary>
            public Table()
            {
            }

            /// <summary>
            /// Creates a new <see cref="Table"/> instance with the provided parameters.
            /// </summary>
            /// <param name="module">The module portion of the name.</param>
            /// <param name="field">The field portion of the name.</param>
            /// <param name="definition">Definiton of the imported table.</param>
            public Table(string module, string field, WebAssembly.Table? definition = null)
                : base(module, field)
            {
                this.Definition = definition;
            }

            /// <summary>
            /// Creates a new <see cref="Table"/> instance with the provided parameters.
            /// </summary>
            /// <param name="module">The module portion of the name.</param>
            /// <param name="field">The field portion of the name.</param>
            /// <param name="minimum">Initial length (in units of table elements or 65,536-byte pages).</param>
            /// <param name="maximum">Maximum length (in units of table elements or 65,536-byte pages).</param>
            public Table(string module, string field, uint minimum, uint? maximum = null)
                : base(module, field)
            {
                this.Definition = new WebAssembly.Table(minimum, maximum);
            }

            /// <summary>
            /// Expresses the value of this instance as a string.
            /// </summary>
            /// <returns>A string representation of this instance.</returns>
            public override string ToString() => $"{base.ToString()} ({nameof(Table)} {Definition}";

            internal sealed override void WriteTo(Writer writer)
            {
                if (Definition == null)
                    throw new InvalidOperationException($"Table {nameof(Definition)} property must be set before writing.");

                base.WriteTo(writer);
                writer.Write((byte)ExternalKind.Table);
                Definition.WriteTo(writer);
            }
        }

        /// <summary>
        /// Describes an imported memory.
        /// </summary>
        public class Memory : Import
        {
            /// <summary>
            /// Always <see cref="ExternalKind.Memory"/>.
            /// </summary>
            public sealed override ExternalKind Kind => ExternalKind.Memory;

            /// <summary>
            /// Type of the imported memory.
            /// </summary>
            public WebAssembly.Memory? Type { get; set; }

            /// <summary>
            /// Creates a new <see cref="Memory"/> instance.
            /// </summary>
            public Memory()
            {
            }

            /// <summary>
            /// Creates a new <see cref="Memory"/> instance with the provided parameters.
            /// </summary>
            /// <param name="module">The module portion of the name.</param>
            /// <param name="field">The field portion of the name.</param>
            /// <param name="type">Type of the imported memory.</param>
            public Memory(string module, string field, WebAssembly.Memory? type = null)
                : base(module, field)
            {
                this.Type = type;
            }

            /// <summary>
            /// Expresses the value of this instance as a string.
            /// </summary>
            /// <returns>A string representation of this instance.</returns>
            public override string ToString() => $"{base.ToString()} ({nameof(Memory)} {Type}";

            internal sealed override void WriteTo(Writer writer)
            {
                if (Type == null)
                    throw new InvalidOperationException("Type must be set before a memory import can be written.");

                base.WriteTo(writer);
                writer.Write((byte)ExternalKind.Memory);
                Type.WriteTo(writer);
            }
        }

        /// <summary>
        /// Describes an imported global.
        /// </summary>
        public class Global : Import
        {
            /// <summary>
            /// Always <see cref="ExternalKind.Global"/>.
            /// </summary>
            public sealed override ExternalKind Kind => ExternalKind.Global;

            /// <summary>
            /// Type of the value.
            /// </summary>
            public WebAssemblyValueType ContentType { get; set; }

            /// <summary>
            /// When true, the value can be changed by running code.
            /// </summary>
            public bool IsMutable { get; set; }

            /// <summary>
            /// Creates a new <see cref="Global"/> instance.
            /// </summary>
            public Global()
            {
                this.ContentType = WebAssemblyValueType.Int32;
            }

            /// <summary>
            /// Creates a new <see cref="Global"/> instance with the provided parameters.
            /// </summary>
            /// <param name="module">The module portion of the name.</param>
            /// <param name="field">The field portion of the name.</param>
            /// <param name="contentType">Type of the value.</param>
            public Global(string module, string field, WebAssemblyValueType contentType = WebAssemblyValueType.Int32)
                : base(module, field)
            {
                this.ContentType = contentType;
            }

            internal Global(Reader reader)
            {
                this.ContentType = (WebAssemblyValueType)reader.ReadVarInt7();
                this.IsMutable = reader.ReadVarUInt1() == 1;
            }

            /// <summary>
            /// Expresses the value of this instance as a string.
            /// </summary>
            /// <returns>A string representation of this instance.</returns>
            public override string ToString() => $"{base.ToString()} ({nameof(Global)} {ContentType} {(IsMutable ? "mutable" : "immutable")}";

            internal sealed override void WriteTo(Writer writer)
            {
                base.WriteTo(writer);
                writer.Write((byte)ExternalKind.Global);
                writer.WriteVar((sbyte)this.ContentType);
                writer.WriteVar(this.IsMutable ? 1u : 0);
            }
        }
    }
}