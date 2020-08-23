using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Linq;

namespace WebAssembly
{
    /// <summary>
    /// Describes the signature of a function.
    /// </summary>
    public class WebAssemblyType : IEquatable<WebAssemblyType>
    {
        /// <summary>
        /// The type of function.  The only accepted value in the initial binary format is <see cref="FunctionType.Function"/>, which is the default.
        /// </summary>
        public FunctionType Form { get; set; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] //Wrapped by a property
        private IList<WebAssemblyValueType>? parameters;

        /// <summary>
        /// Parameters to the function.
        /// </summary>
        /// <exception cref="ArgumentNullException">Value cannot be set to null.</exception>
        public IList<WebAssemblyValueType> Parameters
        {
            get => this.parameters ?? (this.parameters = new List<WebAssemblyValueType>());
            set => this.parameters = value ?? throw new ArgumentNullException(nameof(value));
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] //Wrapped by a property
        private IList<WebAssemblyValueType>? returns;

        /// <summary>
        /// Return types to the function.  For the initial binary format, a maximum of 1 is allowed.
        /// </summary>
        /// <exception cref="ArgumentNullException">Value cannot be set to null.</exception>
        public IList<WebAssemblyValueType> Returns
        {
            get => this.returns ?? (this.returns = new List<WebAssemblyValueType>());
            set => this.returns = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Creates a new <see cref="WebAssemblyType"/> instance.
        /// </summary>
        public WebAssemblyType()
        {
            this.Form = FunctionType.Function;
        }

        internal WebAssemblyType(Reader reader)
        {
            this.Form = (FunctionType)reader.ReadVarInt7();
            int count;

            count = checked((int)reader.ReadVarUInt32());
            var parameters = this.parameters = new List<WebAssemblyValueType>(count);

            for (var i = 0; i < count; i++)
                parameters.Add((WebAssemblyValueType)reader.ReadVarInt7());

            count = checked((int)reader.ReadVarUInt32());
            var returns = this.returns = new List<WebAssemblyValueType>(count);
            for (var i = 0; i < count; i++)
                returns.Add((WebAssemblyValueType)reader.ReadVarInt7());
        }

        /// <summary>
        /// Expresses the value of this instance as a string.
        /// </summary>
        /// <returns>A string representation of this instance.</returns>
        public override string ToString()
        {
            var parameters = this.Parameters;
            var returns = this.Returns;

            var builder = new StringBuilder("Form: ")
                .Append(this.Form.ToString())
                .Append("; Parameters: ")
                ;

            for (var i = 0; i < parameters.Count; i++)
            {
                if (i != 0)
                    builder.Append(", ");
                builder.Append(parameters[i]);
            }

            if (returns.Count == 0)
            {
                builder.Append("; (No Returns)");
            }
            else
            {
                builder.Append("; Returns: ");

                for (var i = 0; i < returns.Count; i++)
                {
                    if (i != 0)
                        builder.Append(", ");
                    builder.Append(returns[i]);
                }
            }

            return builder.ToString();
        }

        internal void WriteTo(Writer writer)
        {
            var parameters = this.Parameters;
            var returns = this.Returns;

            writer.WriteVar((sbyte)this.Form);

            writer.WriteVar((uint)parameters.Count);
            foreach (var parameter in parameters)
                writer.WriteVar((sbyte)parameter);

            writer.WriteVar((uint)returns.Count);
            foreach (var @return in returns)
                writer.WriteVar((sbyte)@return);
        }

        /// <summary>
        /// Creates a hash code based on the parameters and returns of this instance.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            return (int)this.Form ^ HashCode.Combine(this.Parameters.Concat(this.Returns).Select(v => (int)v));
        }

        /// <summary>
        /// Compares the values of this instance for equality with those of another.
        /// </summary>
        /// <param name="obj">The other instance to compare against.</param>
        /// <returns>True if the two instances have the same values, otherwise false.</returns>
        public override bool Equals(object? obj) => this.Equals(obj as WebAssemblyType);

        /// <summary>
        /// Compares the values of this instance for equality with those of another.
        /// </summary>
        /// <param name="other">The other instance to compare against.</param>
        /// <returns>True if the two instances have the same values, otherwise false.</returns>
        public bool Equals(WebAssemblyType? other)
        {
            if (ReferenceEquals(this, other))
                return true;

            if (other == null)
                return false;

            if (this.Form != other.Form)
                return false;

            var thisReturns = this.Returns;
            var otherReturns = other.Returns;

            if (thisReturns.Count != otherReturns.Count)
                return false;

            var thisParameters = this.Parameters;
            var otherParameters = other.Parameters;

            if (thisParameters.Count != otherParameters.Count)
                return false;

            var count = thisReturns.Count;
            for (var i = 0; i < count; i++)
                if (thisReturns[i] != otherReturns[i])
                    return false;

            count = thisParameters.Count;
            for (var i = 0; i < count; i++)
                if (thisParameters[i] != otherParameters[i])
                    return false;

            return true;
        }
    }
}