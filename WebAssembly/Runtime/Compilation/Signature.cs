using System;
using System.Text;

namespace WebAssembly.Runtime.Compilation
{
    internal sealed class Signature : IEquatable<WebAssemblyType>
    {
        public readonly uint TypeIndex;
        public readonly Type[] ParameterTypes;
        public readonly WebAssemblyValueType[] RawParameterTypes;
        public readonly Type[] ReturnTypes;
        public readonly WebAssemblyValueType[] RawReturnTypes;

        private static readonly RegeneratingWeakReference<Signature> empty = new RegeneratingWeakReference<Signature>(() => new Signature());

        public static Signature Empty => empty;

        private Signature()
        {
            this.TypeIndex = uint.MaxValue;
            this.ReturnTypes = this.ParameterTypes = Array.Empty<Type>();
            this.RawReturnTypes = this.RawParameterTypes = Array.Empty<WebAssemblyValueType>();
        }

        public Signature(WebAssemblyValueType returnType)
            : this()
        {
            this.ReturnTypes = new[] { returnType.ToSystemType() };
            this.RawReturnTypes = new[] { returnType };
        }

        public Signature(Reader reader, uint typeIndex)
        {
            this.TypeIndex = typeIndex;

            reader.ReadVarInt7(); //Function Type

            var parameters = this.ParameterTypes = new Type[reader.ReadVarUInt32()];
            var rawParameters = this.RawParameterTypes = new WebAssemblyValueType[parameters.Length];

            for (var i = 0; i < parameters.Length; i++)
                parameters[i] = (rawParameters[i] = (WebAssemblyValueType)reader.ReadVarInt7()).ToSystemType();

            var returns = this.ReturnTypes = new Type[reader.ReadVarUInt1()];
            var rawReturns = this.RawReturnTypes = new WebAssemblyValueType[returns.Length];

            if (returns.Length > 1)
                throw new ModuleLoadException("Multiple returns are not supported.", reader.Offset - 1);

            for (var i = 0; i < returns.Length; i++)
                returns[i] = (rawReturns[i] = (WebAssemblyValueType)reader.ReadVarInt7()).ToSystemType();
        }

        public bool Equals(WebAssemblyType? other)
        {
            if (other == null)
                return false;

            var thisReturns = this.RawReturnTypes;
            var otherReturns = other.Returns;

            if (thisReturns.Length != otherReturns.Count)
                return false;

            var thisParameters = this.RawParameterTypes;
            var otherParameters = other.Parameters;

            if (thisParameters.Length != otherParameters.Count)
                return false;

            for (var i = 0; i < thisReturns.Length; i++)
                if (thisReturns[i] != otherReturns[i])
                    return false;

            for (var i = 0; i < thisParameters.Length; i++)
                if (thisParameters[i] != otherParameters[i])
                    return false;

            return true;
        }

        public override string ToString()
        {
            var parameters = this.ParameterTypes;
            var returns = this.ReturnTypes;

            var builder = new StringBuilder();

            if (parameters.Length == 0)
            {
                builder.Append("(No Parameters)");
            }
            else
            {
                builder.Append("Parameters: ");

                for (var i = 0; i < parameters.Length; i++)
                {
                    if (i != 0)
                        builder.Append(", ");
                    builder.Append(parameters[i]);
                }
            }

            if (returns.Length == 0)
            {
                builder.Append("; (No Returns)");
            }
            else
            {
                builder.Append("; Returns: ");

                for (var i = 0; i < returns.Length; i++)
                {
                    if (i != 0)
                        builder.Append(", ");
                    builder.Append(returns[i]);
                }
            }

            return builder.ToString();
        }
    }
}