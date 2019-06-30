using System;
using System.Linq;
using System.Reflection;

namespace WebAssembly.Runtime
{
    /// <summary>
    /// An array-like structure representing a table import/export, which for the initial specification of WebAssembly is always a function list.
    /// Its behavior mimics the JavaScript version, where the import object is actually modified by the instantiation process.
    /// </summary>
    public class FunctionTable : TableImport
    {
        internal static readonly RegeneratingWeakReference<MethodInfo> IndexGetter = new RegeneratingWeakReference<MethodInfo>(() =>
            typeof(FunctionTable)
            .GetProperties()
            .Where(prop => prop.GetIndexParameters().Length > 0)
            .First()
            .GetGetMethod()
            );

        internal static readonly RegeneratingWeakReference<MethodInfo> IndexSetter = new RegeneratingWeakReference<MethodInfo>(() =>
            typeof(FunctionTable)
            .GetProperties()
            .Where(prop => prop.GetIndexParameters().Length > 0)
            .First()
            .GetSetMethod()
            );

        /// <summary>
        /// Always <see cref="ElementType.AnyFunction"/>.
        /// </summary>
        public sealed override ElementType Element => ElementType.AnyFunction;

        /// <summary>
        /// The initial number of elements.
        /// </summary>
        public uint Initial { get; }

        /// <summary>
        /// The maximum number of elements the table is allowed to grow to.
        /// </summary>
        public uint? Maximum { get; }

        /// <summary>
        /// Creates a new <see cref="FunctionTable"/> with the provided initial size with no maximum.
        /// </summary>
        /// <param name="initial">The initial number of elements.</param>
        public FunctionTable(uint initial)
            : this(initial, null)
        {
        }

        /// <summary>
        /// Creates a new <see cref="FunctionTable"/> with the provided initial and maximum size.
        /// </summary>
        /// <param name="initial">The initial number of elements.</param>
        /// <param name="maximum">The maximum number of elements the table is allowed to grow to.</param>
        /// <exception cref="ArgumentException"><paramref name="initial"/> cannot exceed <paramref name="maximum"/>.</exception>
        public FunctionTable(uint initial, uint? maximum)
        {
            if (initial > maximum.GetValueOrDefault(uint.MaxValue))
                throw new ArgumentException("initial cannot exceed maximum.", nameof(initial));

            this.Initial = initial;
            this.Maximum = maximum;
            this.delegates = new Delegate[initial];
            this.delegateTypes = new System.Type[initial];
        }

        private Delegate[] delegates;
        private System.Type[] delegateTypes;

        /// <summary>
        /// Gets or sets the delegate at the indicated index.  The first time a delegate is provided, it locks in the type for any future reassignments.
        /// </summary>
        /// <param name="index">The index within the table to target.</param>
        /// <returns>The delegate at that index, which may be null.</returns>
        /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> does not fall within the range of the table.</exception>
        /// <exception cref="ArgumentException">The delegate is expected to be of a different type than supplied.</exception>
        /// <remarks>Delegate types set by the compiler come from the provided (or default) <see cref="CompilerConfiguration"/>.</remarks>
        public Delegate this[int index]
        {
            get => this.delegates[index];
            set
            {
                if (value != null)
                {
                    var expectedType = this.delegateTypes[index];
                    var actualType = value.GetType();

                    if (expectedType == null)
                    {
                        this.delegateTypes[index] = actualType;
                    }
                    else if (actualType != expectedType)
                    {
                        var message = $"The delegate at position {index} is expected to be of type {expectedType}, but the supplied delegate is {actualType}.";
                        throw new ArgumentException(message, nameof(value));
                    }
                }

                this.delegates[index] = value;
            }
        }

        /// <summary>
        /// Gets the current size of the table.
        /// </summary>
        public uint Length => (uint)this.delegates.Length;

        /// <summary>
        /// Increases the size of the instance by a specified number of elements.
        /// </summary>
        /// <param name="number">The number of elements you want to grow the table by.</param>
        /// <returns>The previous length of the table.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="number"/>, when added to <see cref="Length"/>, cannot exceed <see cref="Maximum"/>.
        /// </exception>
        public uint Grow(uint number)
        {
            var oldSize = this.Length;
            var newSize = oldSize + number;

            if (newSize > this.Maximum.GetValueOrDefault(uint.MaxValue))
                throw new ArgumentOutOfRangeException(nameof(number), "Adding number to the current Count would exceed the defined Maximum.");

            var checkedSize = checked((int)newSize);
            Array.Resize(ref delegates, checkedSize);
            Array.Resize(ref delegateTypes, checkedSize);

            return oldSize;
        }
    }
}
