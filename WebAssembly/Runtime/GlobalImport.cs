using System;

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
        public Delegate? Setter { get; private set; }

        internal readonly WebAssemblyValueType GetterType;

        /// <summary>
        /// Creates a new <see cref="GlobalImport"/> instance with the provided delegates.
        /// </summary>
        /// <param name="getter">The method to use for read requests.</param>
        /// <param name="setter">The method to use for write requests, or null (the default).</param>
        /// <exception cref="ArgumentNullException"><paramref name="getter"/> cannot be null.</exception>
        public GlobalImport(Func<int> getter, Action<int>? setter = null)
            : this(getter, setter, WebAssemblyValueType.Int32)
        {
        }

        /// <summary>
        /// Creates a new <see cref="GlobalImport"/> instance with the provided delegates.
        /// </summary>
        /// <param name="getter">The method to use for read requests.</param>
        /// <param name="setter">The method to use for write requests, or null (the default).</param>
        /// <exception cref="ArgumentNullException"><paramref name="getter"/> cannot be null.</exception>
        public GlobalImport(Func<uint> getter, Action<uint>? setter = null)
            : this(getter, setter, WebAssemblyValueType.Int32)
        {
        }

        /// <summary>
        /// Creates a new <see cref="GlobalImport"/> instance with the provided delegates.
        /// </summary>
        /// <param name="getter">The method to use for read requests.</param>
        /// <param name="setter">The method to use for write requests, or null (the default).</param>
        /// <exception cref="ArgumentNullException"><paramref name="getter"/> cannot be null.</exception>
        public GlobalImport(Func<long> getter, Action<long>? setter = null)
            : this(getter, setter, WebAssemblyValueType.Int64)
        {
        }

        /// <summary>
        /// Creates a new <see cref="GlobalImport"/> instance with the provided delegates.
        /// </summary>
        /// <param name="getter">The method to use for read requests.</param>
        /// <param name="setter">The method to use for write requests, or null (the default).</param>
        /// <exception cref="ArgumentNullException"><paramref name="getter"/> cannot be null.</exception>
        public GlobalImport(Func<ulong> getter, Action<ulong>? setter = null)
            : this(getter, setter, WebAssemblyValueType.Int64)
        {
        }

        /// <summary>
        /// Creates a new <see cref="GlobalImport"/> instance with the provided delegates.
        /// </summary>
        /// <param name="getter">The method to use for read requests.</param>
        /// <param name="setter">The method to use for write requests, or null (the default).</param>
        /// <exception cref="ArgumentNullException"><paramref name="getter"/> cannot be null.</exception>
        public GlobalImport(Func<float> getter, Action<float>? setter = null)
            : this(getter, setter, WebAssemblyValueType.Float32)
        {
        }

        /// <summary>
        /// Creates a new <see cref="GlobalImport"/> instance with the provided delegates.
        /// </summary>
        /// <param name="getter">The method to use for read requests.</param>
        /// <param name="setter">The method to use for write requests, or null (the default).</param>
        /// <exception cref="ArgumentNullException"><paramref name="getter"/> cannot be null.</exception>
        public GlobalImport(Func<double> getter, Action<double>? setter = null)
            : this(getter, setter, WebAssemblyValueType.Float64)
        {
        }

        private GlobalImport(Delegate getter, Delegate? setter, WebAssemblyValueType type)
        {
            if (getter == null)
                throw new ArgumentNullException(nameof(getter));

            this.Getter = getter;
            this.Setter = setter;
            this.GetterType = type;
        }

        static WebAssemblyValueType DetermineValueType(Delegate getter, Delegate? setter)
        {
            if (getter == null)
                throw new ArgumentNullException(nameof(getter));

            if (getter.Method.GetParameters().Length != 0)
                throw new ArgumentException("getter cannot have parameters.", nameof(getter));

            var gret = getter.Method.ReturnType;

            if (gret == null || !gret.TryConvertToValueType(out var gtype))
                throw new ArgumentException("getter does not return a compatible type.", nameof(getter));

            if (setter != null)
            {
                if (setter.Method.ReturnType != null)
                    throw new ArgumentException("setter cannot have a return type.", nameof(setter));

                var sparms = setter.Method.GetParameters();
                if (sparms.Length != 1)
                    throw new ArgumentException("setter must have exactly 1 parameter.", nameof(setter));

                var sparm = sparms[0].ParameterType;
                if (sparm == null || !sparm.TryConvertToValueType(out var stype))
                    throw new ArgumentException("setter does not accept a compatible type.", nameof(setter));

                if (stype != gtype)
                    throw new ArgumentException("setter does not accept the same type as getter returns.", nameof(setter));
            }

            return gtype;
        }

        /// <summary>
        /// Creates a new <see cref="GlobalImport"/> instance with the provided delegates.
        /// </summary>
        /// <param name="getter">The method to use for read requests.</param>
        /// <param name="setter">The method to use for write requests, or null (the default).</param>
        /// <exception cref="ArgumentNullException"><paramref name="getter"/> cannot be null.</exception>
        /// <exception cref="ArgumentException"><paramref name="getter"/> and/or <paramref name="setter"/> are incompatible.</exception>
        public GlobalImport(Delegate getter, Delegate? setter = null)
            : this(getter, setter, DetermineValueType(getter, setter))
        {
        }
    }
}