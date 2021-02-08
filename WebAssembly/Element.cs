using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace WebAssembly
{
    /// <summary>
    /// The elements section allows a module to initialize (at instantiation time) the elements of any imported or internally-defined table with any other definition in the module.
    /// </summary>
    public class Element
    {
        /// <summary>
        /// The table index.
        /// </summary>
        public uint Index { get; set; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] //Wrapped by a property
        private IList<Instruction>? initializerExpression;

        /// <summary>
        /// An initializer expression that computes the offset at which to place the elements.
        /// </summary>
        /// <exception cref="ArgumentNullException">Value cannot be set to null.</exception>
        public IList<Instruction> InitializerExpression
        {
            get => this.initializerExpression ??= new List<Instruction>();
            set => this.initializerExpression = value ?? throw new ArgumentNullException(nameof(value));
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] //Wrapped by a property
        private IList<uint>? elements;

        /// <summary>
        /// A sequence of function indices.
        /// </summary>
        /// <exception cref="ArgumentNullException">Value cannot be set to null.</exception>
        public IList<uint> Elements
        {
            get => this.elements ??= new List<uint>();
            set => this.elements = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Creates a new <see cref="Element"/> instance.
        /// </summary>
        public Element()
        {
        }

        internal Element(Reader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            this.Index = reader.ReadVarUInt32();
            this.initializerExpression = Instruction.ParseInitializerExpression(reader).ToList();

            var count = checked((int)reader.ReadVarUInt32());
            var elements = this.elements = new List<uint>();

            for (var i = 0; i < count; i++)
                elements.Add(reader.ReadVarUInt32());
        }

        /// <summary>
        /// Expresses the value of this instance as a string.
        /// </summary>
        /// <returns>A string representation of this instance.</returns>
        public override string ToString() => $"{Index}: {InitializerExpression.Count} ({Elements.Count})";

        internal void WriteTo(Writer writer)
        {
            writer.WriteVar(this.Index);
            foreach (var instruction in this.InitializerExpression)
                instruction.WriteTo(writer);

            writer.WriteVar((uint)this.Elements.Count);
            foreach (var element in this.Elements)
                writer.WriteVar(element);
        }
    }
}