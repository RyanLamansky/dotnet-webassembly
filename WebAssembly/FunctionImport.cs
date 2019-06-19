using System;
using System.Reflection;

namespace WebAssembly
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

        internal readonly Type Type;

        /// <summary>
        /// Creates a new <see cref="FunctionImport"/> instance with the provided <see cref="Delegate"/>.
        /// </summary>
        /// <param name="moduleName">The first portion of the two part name.</param>
        /// <param name="exportName">The second portion of the two-part name.</param>
        /// <param name="del">The delegate to use for the import.</param>
        /// <exception cref="ArgumentNullException">No parameters can be null.</exception>
        /// <exception cref="ArgumentException">A parameter or return type is not compatible with WebAssembly.</exception>
        public FunctionImport(string moduleName, string exportName, Delegate del)
            : base(moduleName, exportName)
        {
            if (del == null)
                throw new ArgumentNullException(nameof(del));

            var method = (this.Method = del).GetMethodInfo();

            this.Type = new Type();
            if (method.ReturnType != typeof(void))
            {
                if (!method.ReturnType.TryConvertToValueType(out var type))
                    throw new ArgumentException($"Return type {method.ReturnType} is not compatible with WebAssembly.", nameof(method));

                this.Type.Returns = new[] { type };
            }

            foreach (var parameter in method.GetParameters())
            {
                if (!parameter.ParameterType.TryConvertToValueType(out var type))
                    throw new ArgumentException($"Parameter type {parameter} is not compatible with WebAssembly.", nameof(method));

                this.Type.Parameters.Add(type);
            }
        }
    }
}