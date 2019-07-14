using System;
using System.Reflection;

namespace WebAssembly.Runtime
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

        internal readonly WebAssemblyValueType GetterType;

        /// <summary>
        /// Creates a new <see cref="GlobalImport"/> instance with the provided delegates.
        /// </summary>
        /// <param name="getter">The method to use for read requests.</param>
        /// <param name="setter">The method to use for write requests, or null (the default).</param>
        /// <exception cref="ArgumentNullException"><paramref name="getter"/> cannot be null.</exception>
        public GlobalImport(Func<int> getter, Action<int> setter = null)
            : this( getter, setter, WebAssemblyValueType.Int32)
        {
        }

        /// <summary>
        /// Creates a new <see cref="GlobalImport"/> instance with the provided delegates.
        /// </summary>
        /// <param name="getter">The method to use for read requests.</param>
        /// <param name="setter">The method to use for write requests, or null (the default).</param>
        /// <exception cref="ArgumentNullException"><paramref name="getter"/> cannot be null.</exception>
        public GlobalImport(Func<uint> getter, Action<uint> setter = null)
            : this(getter, setter, WebAssemblyValueType.Int32)
        {
        }

        /// <summary>
        /// Creates a new <see cref="GlobalImport"/> instance with the provided delegates.
        /// </summary>
        /// <param name="getter">The method to use for read requests.</param>
        /// <param name="setter">The method to use for write requests, or null (the default).</param>
        /// <exception cref="ArgumentNullException"><paramref name="getter"/> cannot be null.</exception>
        public GlobalImport(Func<long> getter, Action<long> setter = null)
            : this(getter, setter, WebAssemblyValueType.Int64)
        {
        }

        /// <summary>
        /// Creates a new <see cref="GlobalImport"/> instance with the provided delegates.
        /// </summary>
        /// <param name="getter">The method to use for read requests.</param>
        /// <param name="setter">The method to use for write requests, or null (the default).</param>
        /// <exception cref="ArgumentNullException"><paramref name="getter"/> cannot be null.</exception>
        public GlobalImport(Func<ulong> getter, Action<ulong> setter = null)
            : this(getter, setter, WebAssemblyValueType.Int64)
        {
        }

        /// <summary>
        /// Creates a new <see cref="GlobalImport"/> instance with the provided delegates.
        /// </summary>
        /// <param name="getter">The method to use for read requests.</param>
        /// <param name="setter">The method to use for write requests, or null (the default).</param>
        /// <exception cref="ArgumentNullException"><paramref name="getter"/> cannot be null.</exception>
        public GlobalImport(Func<float> getter, Action<float> setter = null)
            : this(getter, setter, WebAssemblyValueType.Float32)
        {
        }

        /// <summary>
        /// Creates a new <see cref="GlobalImport"/> instance with the provided delegates.
        /// </summary>
        /// <param name="getter">The method to use for read requests.</param>
        /// <param name="setter">The method to use for write requests, or null (the default).</param>
        /// <exception cref="ArgumentNullException"><paramref name="getter"/> cannot be null.</exception>
        public GlobalImport(Func<double> getter, Action<double> setter = null)
            : this(getter, setter, WebAssemblyValueType.Float64)
        {
        }

        private GlobalImport(Delegate getter, Delegate setter, WebAssemblyValueType type)
        {
            this.Getter = getter;
            this.Setter = setter;
            this.GetterType = type;
        }
    }
}