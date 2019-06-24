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
        public Delegate Getter { get; private set; }

        /// <summary>
        /// The method to use for write requests, or null (the default).
        /// </summary>
        public Delegate Setter { get; private set; }

        internal readonly ValueType GetterType;

        /// <summary>
        /// Creates a new <see cref="GlobalImport"/> instance with the provided delegates.
        /// </summary>
        /// <param name="moduleName">The first portion of the two part name.</param>
        /// <param name="exportName">The second portion of the two-part name.</param>
        /// <param name="getter">The method to use for read requests.</param>
        /// <param name="setter">The method to use for write requests, or null (the default).</param>
        /// <exception cref="ArgumentNullException"><paramref name="moduleName"/>, <paramref name="exportName"/>, and <paramref name="getter"/> cannot be null.</exception>
        public GlobalImport(string moduleName, string exportName, Func<int> getter, Action<int> setter = null)
            : this(moduleName, exportName, getter, setter, ValueType.Int32)
        {
        }

        /// <summary>
        /// Creates a new <see cref="GlobalImport"/> instance with the provided delegates.
        /// </summary>
        /// <param name="moduleName">The first portion of the two part name.</param>
        /// <param name="exportName">The second portion of the two-part name.</param>
        /// <param name="getter">The method to use for read requests.</param>
        /// <param name="setter">The method to use for write requests, or null (the default).</param>
        /// <exception cref="ArgumentNullException"><paramref name="moduleName"/>, <paramref name="exportName"/>, and <paramref name="getter"/> cannot be null.</exception>
        public GlobalImport(string moduleName, string exportName, Func<uint> getter, Action<uint> setter = null)
            : this(moduleName, exportName, getter, setter, ValueType.Int32)
        {
        }

        /// <summary>
        /// Creates a new <see cref="GlobalImport"/> instance with the provided delegates.
        /// </summary>
        /// <param name="moduleName">The first portion of the two part name.</param>
        /// <param name="exportName">The second portion of the two-part name.</param>
        /// <param name="getter">The method to use for read requests.</param>
        /// <param name="setter">The method to use for write requests, or null (the default).</param>
        /// <exception cref="ArgumentNullException"><paramref name="moduleName"/>, <paramref name="exportName"/>, and <paramref name="getter"/> cannot be null.</exception>
        public GlobalImport(string moduleName, string exportName, Func<long> getter, Action<long> setter = null)
            : this(moduleName, exportName, getter, setter, ValueType.Int64)
        {
        }

        /// <summary>
        /// Creates a new <see cref="GlobalImport"/> instance with the provided delegates.
        /// </summary>
        /// <param name="moduleName">The first portion of the two part name.</param>
        /// <param name="exportName">The second portion of the two-part name.</param>
        /// <param name="getter">The method to use for read requests.</param>
        /// <param name="setter">The method to use for write requests, or null (the default).</param>
        /// <exception cref="ArgumentNullException"><paramref name="moduleName"/>, <paramref name="exportName"/>, and <paramref name="getter"/> cannot be null.</exception>
        public GlobalImport(string moduleName, string exportName, Func<ulong> getter, Action<ulong> setter = null)
            : this(moduleName, exportName, getter, setter, ValueType.Int64)
        {
        }

        /// <summary>
        /// Creates a new <see cref="GlobalImport"/> instance with the provided delegates.
        /// </summary>
        /// <param name="moduleName">The first portion of the two part name.</param>
        /// <param name="exportName">The second portion of the two-part name.</param>
        /// <param name="getter">The method to use for read requests.</param>
        /// <param name="setter">The method to use for write requests, or null (the default).</param>
        /// <exception cref="ArgumentNullException"><paramref name="moduleName"/>, <paramref name="exportName"/>, and <paramref name="getter"/> cannot be null.</exception>
        public GlobalImport(string moduleName, string exportName, Func<float> getter, Action<float> setter = null)
            : this(moduleName, exportName, getter, setter, ValueType.Float32)
        {
        }

        /// <summary>
        /// Creates a new <see cref="GlobalImport"/> instance with the provided delegates.
        /// </summary>
        /// <param name="moduleName">The first portion of the two part name.</param>
        /// <param name="exportName">The second portion of the two-part name.</param>
        /// <param name="getter">The method to use for read requests.</param>
        /// <param name="setter">The method to use for write requests, or null (the default).</param>
        /// <exception cref="ArgumentNullException"><paramref name="moduleName"/>, <paramref name="exportName"/>, and <paramref name="getter"/> cannot be null.</exception>
        public GlobalImport(string moduleName, string exportName, Func<double> getter, Action<double> setter = null)
            : this(moduleName, exportName, getter, setter, ValueType.Float64)
        {
        }

        private GlobalImport(string moduleName, string exportName, Delegate getter, Delegate setter, ValueType type)
            : base(moduleName, exportName)
        {
            this.Getter = getter;
            this.Setter = setter;
            this.GetterType = type;
        }
    }
}