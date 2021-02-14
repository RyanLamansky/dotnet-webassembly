using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace WebAssembly
{
    /// <summary>
    /// Function bodies consist of a sequence of local variable declarations followed by bytecode instructions.
    /// </summary>
    public class FunctionBody : IEquatable<FunctionBody>
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] //Wrapped by a property
        private IList<Local>? locals;

        /// <summary>
        /// Local variables.
        /// </summary>
        /// <exception cref="ArgumentNullException">Value cannot be set to null.</exception>
        public IList<Local> Locals
        {
            get => this.locals ??= new List<Local>();
            set => this.locals = value ?? throw new ArgumentNullException(nameof(value));
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] //Wrapped by a property
        private IList<Instruction>? code;

        /// <summary>
        /// Bytecode of the function.
        /// </summary>
        /// <exception cref="ArgumentNullException">Value cannot be set to null.</exception>
        public IList<Instruction> Code
        {
            get => this.code ??= new List<Instruction>();
            set => this.code = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Creates a new instance of <see cref="FunctionBody"/>.
        /// </summary>
        public FunctionBody()
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="FunctionBody"/> with the provided code.
        /// </summary>
        /// <param name="code"></param>
        /// <exception cref="ArgumentNullException"><paramref name="code"/> cannot be null.</exception>
        public FunctionBody(params Instruction[] code)
        {
            this.code = code ?? throw new ArgumentNullException(nameof(code));
        }

        /// <summary>
        /// Creates a new instance of <see cref="FunctionBody"/> with the provided locals.
        /// </summary>
        /// <param name="locals"></param>
        /// <exception cref="ArgumentNullException"><paramref name="locals"/> cannot be null.</exception>
        public FunctionBody(params Local[] locals)
        {
            this.locals = locals ?? throw new ArgumentNullException(nameof(locals));
        }

        internal FunctionBody(Reader reader, long byteLength)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            var startingOffset = reader.Offset;
            var localCount = reader.ReadVarUInt32();
            var locals = this.Locals = new List<Local>(checked((int)localCount));
            for (var i = 0; i < localCount; i++)
                locals.Add(new Local(reader));

            this.code = Instruction.Parse(reader).ToArray();

            if (reader.Offset - startingOffset != byteLength)
                throw new ModuleLoadException($"Instruction sequence reader ended after readering {reader.Offset - startingOffset} characters, expected {byteLength}.", reader.Offset);
        }

        /// <summary>
        /// Expresses the value of this instance as a string.
        /// </summary>
        /// <returns>A string representation of this instance.</returns>
        public override string ToString() => $"Locals: {locals?.Count}, Code: {code?.Count}";

        /// <summary>
        /// Returns a hash code based on the value of this instance.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
            => HashCode.Combine(
                this.Locals.Select(local => local?.GetHashCode())
                .Concat(this.Code.Select(instruction => instruction?.GetHashCode()
                )));

        /// <summary>
        /// Determines whether this instance is identical to another.
        /// </summary>
        /// <param name="obj">The object instance to compare against.</param>
        /// <returns>True if they have the same type and value, otherwise false.</returns>
        public override bool Equals(object? obj) => this.Equals(obj as FunctionBody);

        /// <summary>
        /// Determines whether this instance is identical to another.
        /// </summary>
        /// <param name="other">The instance to compare against.</param>
        /// <returns>True if they have the same type and value, otherwise false.</returns>
        public bool Equals(FunctionBody? other)
        {
            if (other == null)
                return false;

            using (var items = this.Code.GetEnumerator())
            using (var others = this.Code.GetEnumerator())
            {
                bool itemMoved, othersMoved;
                while ((itemMoved = items.MoveNext()) & (othersMoved = others.MoveNext()))
                {
                    if (itemMoved & !othersMoved)
                        return false;

                    if (!items.Current.Equals(others.Current))
                        return false;
                }

                return !itemMoved && !othersMoved;
            };
        }

        internal void WriteTo(Writer writer, byte[] buffer)
        {
            using var memory = new MemoryStream();
            using (var bodyWriter = new Writer(memory))
            {
                var locals = this.Locals;
                var instructions = this.Code;

                bodyWriter.WriteVar((uint)locals.Count);
                foreach (var local in locals)
                    local.WriteTo(bodyWriter);

                foreach (var instruction in instructions)
                    instruction.WriteTo(bodyWriter);
            }

            writer.WriteVar(checked((uint)memory.Length));
            memory.Position = 0;
            int read;
            while ((read = memory.Read(buffer, 0, buffer.Length)) > 0)
                writer.Write(buffer, 0, read);
        }
    }
}