using System;
using System.Reflection;

namespace WebAssembly.Runtime
{
    /// <summary>
    /// Indicates a method to use for a WebAssembly function import.
    /// </summary>
    public class FunctionImport : RuntimeImport
    {
        /// <summary>
        /// Always <see cref="ExternalKind.Function"/>.
        /// </summary>
        public sealed override ExternalKind Kind => ExternalKind.Function;

        /// <summary>
        /// The method to use for the import.
        /// </summary>
        public Delegate Method { get; }

        internal readonly WebAssemblyType Type;

        /// <summary>
        /// Creates a new <see cref="FunctionImport"/> instance with the provided <see cref="Delegate"/>.
        /// </summary>
        /// <param name="del">The delegate to use for the import.</param>
        /// <exception cref="ArgumentNullException">No parameters can be null.</exception>
        /// <exception cref="ArgumentException">A parameter or return type is not compatible with WebAssembly.</exception>
        public FunctionImport(Delegate del)
        {
            if (del == null)
                throw new ArgumentNullException(nameof(del));

            var method = (this.Method = del).GetMethodInfo();
            if (method == null)
                throw new ArgumentException("Provided delegate isn't associated with a method.", nameof(del));

            this.Type = new WebAssemblyType();
            if (method.ReturnType != typeof(void))
            {
                if (!method.ReturnType.TryConvertToValueType(out var type))
                    throw new ArgumentException($"Return type {method.ReturnType} is not compatible with WebAssembly.", nameof(del));

                this.Type.Returns = new[] { type };
            }

            foreach (var parameter in method.GetParameters())
            {
                if (!parameter.ParameterType.TryConvertToValueType(out var type))
                    throw new ArgumentException($"Parameter type {parameter} is not compatible with WebAssembly.", nameof(del));

                this.Type.Parameters.Add(type);
            }
        }
    }
}