using System;
using System.Reflection;

namespace WebAssembly
{
    /// <summary>
    /// Indicates the method(s) to use for a WebAssembly global import.
    /// </summary>
    public class GlobalImport : RuntimeImport
    {
        /// <summary>
        /// Always <see cref="ExternalKind.Global"/>.
        /// </summary>
        public sealed override ExternalKind Kind => ExternalKind.Global;

        /// <summary>
        /// The method to use for read requests.
        /// </summary>
        public MethodInfo Getter { get; private set; }

        /// <summary>
        /// The method to use for write requests, or null (the default).
        /// </summary>
        public MethodInfo Setter { get; private set; }

        internal readonly ValueType GetterType;

        /// <summary>
        /// Creates a new <see cref="GlobalImport"/> instance with the provided <see cref="PropertyInfo"/>'s getter and (if present and compatible) setter.
        /// </summary>
        /// <param name="moduleName">The first portion of the two part name.</param>
        /// <param name="exportName">The second portion of the two-part name.</param>
        /// <param name="property">The property to use for the import.  If the setter is not compatible, it will be ignored.</param>
        /// <exception cref="ArgumentNullException">No parameters can be null.</exception>
        /// <exception cref="ArgumentException"><paramref name="property"/> must have a public, static getter.</exception>
        public GlobalImport(string moduleName, string exportName, PropertyInfo property)
            : this(
                  moduleName,
                  exportName,
                  (property ?? throw new ArgumentNullException(nameof(property))).GetMethod,
                  NullifyIfNotCompatible(property.SetMethod))
        {
        }

        static MethodInfo NullifyIfNotCompatible(MethodInfo setter)
        {
            ParameterInfo[] parameters;
            if (setter != null
                && setter.IsStatic
                && setter.IsPublic
                && setter.ReturnType == typeof(void)
                && (parameters = setter.GetParameters()).Length == 1
                && parameters[0].ParameterType.IsSupported()
                )
            {
                return setter;
            }

            return null;
        }

        /// <summary>
        /// Creates a new <see cref="GlobalImport"/> instance with the provided <see cref="PropertyInfo"/>.
        /// </summary>
        /// <param name="moduleName">The first portion of the two part name.</param>
        /// <param name="exportName">The second portion of the two-part name.</param>
        /// <param name="getter">The method to use for read requests.</param>
        /// <param name="setter">The method to use for write requests, or null (the default).</param>
        /// <exception cref="ArgumentNullException"><paramref name="moduleName"/>, <paramref name="exportName"/>, and <paramref name="getter"/> cannot be null.</exception>
        /// <exception cref="ArgumentException"><paramref name="getter"/> (and <paramref name="setter"/> if not null) must be public and static.</exception>
        public GlobalImport(string moduleName, string exportName, MethodInfo getter, MethodInfo setter = null)
            : base(moduleName, exportName)
        {
            if (getter == null)
                throw new ArgumentNullException(nameof(getter));

            if (getter.IsStatic == false || getter.IsPublic == false)
                throw new ArgumentException("Imported methods must be public and static.", nameof(getter));

            if (getter.GetParameters().Length != 0)
                throw new ArgumentException("Imported getters must not have parameters.", nameof(getter));

            if (!getter.ReturnType.TryConvertToValueType(out var returnType))
                throw new ArgumentException($"Return type {getter.ReturnType} is not compatible with WebAssembly.", nameof(getter));

            this.GetterType = returnType;

            this.Getter = getter;

            if (setter == null)
                return;

            if (setter.IsStatic == false || setter.IsPublic == false)
                throw new ArgumentException("Imported methods must be public and static.", nameof(setter));

            var parameters = setter.GetParameters();
            if (parameters.Length != 1)
                throw new ArgumentException("Imported setters must have 1 parameter.", nameof(setter));

            if (parameters[0].ParameterType != getter.ReturnType)
                throw new ArgumentException("Imported setter must have the same parameter type as the getter return.", nameof(setter));

            this.Setter = setter;
        }
    }
}