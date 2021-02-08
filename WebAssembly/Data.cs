using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace WebAssembly
{
    /// <summary>
    /// The data section declares the initialized data that is loaded into the linear memory.
    /// </summary>
    public class Data
    {
        /// <summary>
        /// The linear memory index (always 0 in the initial version of WebAssembly).
        /// </summary>
        public uint Index { get; set; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] //Wrapped by a property
        private IList<Instruction>? initializerExpression;

        /// <summary>
        /// An <see cref="WebAssemblyValueType.Int32"/> initializer expression that computes the offset at which to place the data.
        /// </summary>
        /// <exception cref="ArgumentNullException">Value cannot be set to null.</exception>
        public IList<Instruction> InitializerExpression
        {
            get => this.initializerExpression ??= new List<Instruction>();
            set => this.initializerExpression = value ?? throw new ArgumentNullException(nameof(value));
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] //Wrapped by a property
        private IList<byte>? rawData;

        /// <summary>
        /// Raw data in byte form.
        /// </summary>
        /// <exception cref="ArgumentNullException">Value cannot be set to null.</exception>
        public IList<byte> RawData
        {
            get => this.rawData ??= new List<byte>();
            set => this.rawData = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Creates a new <see cref="Data"/> instance.
        /// </summary>
        public Data()
        {
        }

        internal Data(Reader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            this.Index = reader.ReadVarUInt32();
            this.initializerExpression = Instruction.ParseInitializerExpression(reader).ToList();
            this.rawData = reader.ReadBytes(reader.ReadVarUInt32());
        }

        /// <summary>
        /// Expresses the value of this instance as a string.
        /// </summary>
        /// <returns>A string representation of this instance.</returns>
        public override string ToString() => $"Index: {Index}, Length: {rawData?.Count}";

        internal void WriteTo(Writer writer)
        {
            writer.WriteVar(this.Index);
            foreach (var instruction in this.InitializerExpression)
                instruction.WriteTo(writer);

            writer.WriteVar((uint)this.RawData.Count);
            if (this.RawData is byte[] bytes)
                writer.Write(bytes);
            else
                foreach (var b in this.RawData)
                    writer.Write(b);
        }
    }
}