using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace WebAssembly
{
    /// <summary>
    /// Describes a global variable.
    /// </summary>
    public class Global
    {
        /// <summary>
        /// Type of the value.
        /// </summary>
        public ValueType ContentType { get; set; }

        /// <summary>
        /// When true, the value can be changed by running code.
        /// </summary>
        public bool IsMutable { get; set; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] //Wrapped by a property
        private IList<Instruction> initializerExpression;

        /// <summary>
        /// The initial value of the global.
        /// </summary>
        /// <exception cref="ArgumentNullException">Value cannot be set to null.</exception>
        public IList<Instruction> InitializerExpression
        {
            get => this.initializerExpression ?? (this.initializerExpression = new List<Instruction>());
            set => this.initializerExpression = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Creates a new <see cref="Global"/> instance.
        /// </summary>
        public Global()
        {
        }

        internal Global(Reader reader)
        {
            this.ContentType = (ValueType)reader.ReadVarInt7();
            this.IsMutable = reader.ReadVarUInt1() == 1;
            this.InitializerExpression = Instruction.ParseInitializerExpression(reader).ToList();
        }

        internal void WriteTo(Writer writer)
        {
            writer.WriteVar((sbyte)this.ContentType);
            writer.WriteVar(this.IsMutable ? 1u : 0);
            foreach (var instruction in this.InitializerExpression)
                instruction.WriteTo(writer);
        }
    }
}