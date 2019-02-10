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
        public MethodInfo Method { get; private set; }

        internal readonly Type Type;

        /// <summary>
        /// Creates a new <see cref="FunctionImport"/> instance with the provided <see cref="MethodInfo"/>.
        /// </summary>
        /// <param name="moduleName">The first portion of the two part name.</param>
        /// <param name="exportName">The second portion of the two-part name.</param>
        /// <param name="method">The method to use for the import.</param>
        /// <exception cref="ArgumentNullException">No parameters can be null.</exception>
        /// <exception cref="ArgumentException"><paramref name="method"/> must be public, static, and cannot be a <see cref="System.Reflection.Emit.MethodBuilder"/>.</exception>
        public FunctionImport(string moduleName, string exportName, MethodInfo method)
            : base(moduleName, exportName)
        {
            if (method == null)
                throw new ArgumentNullException(nameof(method));

            if (method.IsStatic == false || method.IsPublic == false)
                throw new ArgumentException("Imported methods must be public and static.", nameof(method));

            if (method is System.Reflection.Emit.MethodBuilder)
                throw new ArgumentException("Imported methods cannot be dynamic method builders.", nameof(method));

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

            this.Method = method;
        }
    }
}