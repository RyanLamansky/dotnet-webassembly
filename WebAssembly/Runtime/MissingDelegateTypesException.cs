using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace WebAssembly.Runtime
{
    /// <summary>
    /// Thrown when <see cref="CompilerConfiguration.GetDelegateForType"/> fails to find a delegate type for one or more imports.
    /// </summary>
    public sealed class MissingDelegateTypesException : CompilerException
    {
        /// <summary>
        /// The characteristics of required delegates that were not found.
        /// </summary>
        public IReadOnlyCollection<MissingDelegateType> MissingDelegateTypes { get; }

        internal MissingDelegateTypesException(List<MissingDelegateType> missing)
            : base($"Configuration {nameof(CompilerConfiguration.GetDelegateForType)} could not provide { string.Join(", ", missing)}.")
        {
            this.MissingDelegateTypes = new ReadOnlyCollection<MissingDelegateType>(missing);
        }
    }

    /// <summary>
    /// Describes a delegate that was required but was not provided.
    /// </summary>
    public sealed class MissingDelegateType
    {
        /// <summary>
        /// The module portion of the name.
        /// </summary>
        public string Module { get; }

        /// <summary>
        /// The field portion of the name.
        /// </summary>
        public string Field { get; }

        /// <summary>
        /// The number of parameters needed for the delegate.
        /// </summary>
        public int Parameters { get; }

        /// <summary>
        /// The number of returns needed for the delegate.
        /// </summary>
        public int Returns { get; }

        internal MissingDelegateType(string module, string field, Signature signature)
        {
            this.Module = module;
            this.Field = field;
            this.Parameters = signature.ParameterTypes.Length;
            this.Returns = signature.ReturnTypes.Length;
        }

        /// <summary>
        /// Expresses the value of this instance as a string.
        /// </summary>
        /// <returns>A string representation of this instance.</returns>
        public override string ToString()
        {
            var builder = new StringBuilder()
                .Append(Parameters)
                .Append(" parameter")
                ;

            if (Parameters != 1)
                builder.Append('s');

            builder.Append(" and ");

            switch (Returns)
            {
                case 0:
                    builder.Append("no returns");
                    break;
                case 1:
                    builder.Append("one return");
                    break;
                default: // Not possible in "MVP"-level WASM but maybe in the future.
                    builder
                        .Append(Returns)
                        .Append(" returns")
                        ;
                    break;
            }

            builder
                .Append(" for ")
                .Append(Module)
                .Append("::")
                .Append(Field)
                ;

            return builder.ToString();
        }
    }
}
