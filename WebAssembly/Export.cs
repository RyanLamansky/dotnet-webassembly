using System;
using System.Diagnostics;

namespace WebAssembly
{
    /// <summary>
    /// Makes a WebAssembly feature accessible to the host environment.
    /// </summary>
    public class Export
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] //Wrapped by a property
        private string? name;

        /// <summary>
        /// The name of the exported item.
        /// </summary>
        /// <exception cref="ArgumentNullException">Value cannot be set to null.</exception>
        public string Name
        {
            get => name ?? "";
            set => name = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// The kind of definition being exported.
        /// </summary>
        public ExternalKind Kind { get; set; }

        /// <summary>
        /// The index into the corresponding index space.
        /// </summary>
        public uint Index { get; set; }

        /// <summary>
        /// Creates a new <see cref="Export"/>  instance.
        /// </summary>
        public Export()
        {
        }

        /// <summary>
        /// Creates a new <see cref="Export"/>  instance with the provided parameters.
        /// </summary>
        /// <param name="name">The name of the exported item.</param>
        /// <param name="index">The index into the corresponding index space.</param>
        /// <param name="kind"></param>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> cannot be null.</exception>
        public Export(string name, uint index = 0, ExternalKind kind = ExternalKind.Function)
        {
            this.name = name ?? throw new ArgumentNullException(nameof(name));
            this.Index = index;
            this.Kind = kind;
        }

        /// <summary>
        /// Creates a new <see cref="Export"/> instance from the provided <see cref="Reader"/>.
        /// </summary>
        /// <param name="reader">Provides raw data.</param>
        internal Export(Reader reader)
        {
            this.Name = reader.ReadString(reader.ReadVarUInt32());
            this.Kind = (ExternalKind)reader.ReadByte();
            this.Index = reader.ReadVarUInt32();
        }

        /// <summary>
        /// Expresses the value of this instance as a string.
        /// </summary>
        /// <returns>A string representation of this instance.</returns>
        public override string ToString() => $"{Name}; {Kind}: {Index}";

        internal void WriteTo(Writer writer)
        {
            writer.Write(this.Name);
            writer.Write((byte)this.Kind);
            writer.WriteVar(this.Index);
        }
    }
}