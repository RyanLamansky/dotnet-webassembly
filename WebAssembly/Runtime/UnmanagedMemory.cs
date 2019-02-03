using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace WebAssembly.Runtime
{
    /// <summary>
    /// Implements a WebAssembly linear memory resource using unmanaged memory.
    /// </summary>
    public sealed class UnmanagedMemory : IDisposable
    {
        internal static readonly RegeneratingWeakReference<MethodInfo> SizeGetter = new RegeneratingWeakReference<MethodInfo>(()
            => typeof(UnmanagedMemory).GetTypeInfo().DeclaredProperties.Where(prop => prop.Name == nameof(Size)).First().GetMethod);
        internal static readonly RegeneratingWeakReference<MethodInfo> StartGetter = new RegeneratingWeakReference<MethodInfo>(()
            => typeof(UnmanagedMemory).GetTypeInfo().DeclaredProperties.Where(prop => prop.Name == nameof(Start)).First().GetMethod);
        internal static readonly RegeneratingWeakReference<MethodInfo> GrowMethod = new RegeneratingWeakReference<MethodInfo>(()
            => typeof(UnmanagedMemory).GetTypeInfo().DeclaredMethods.Where(prop => prop.Name == nameof(Grow)).First());
        private static readonly RegeneratingWeakReference<long[]> emptyPage = new RegeneratingWeakReference<long[]>(() => new long[Memory.PageSize / 8]);

        /// <summary>
        /// Creates a new <see cref="UnmanagedMemory"/> instance with the provided parameters.
        /// </summary>
        /// <param name="minimum">The initial size of memory in <see cref="Memory.PageSize"/> pages.</param>
        /// <param name="maximum">The maximum size of memory in <see cref="Memory.PageSize"/> pages.</param>
        public UnmanagedMemory(uint minimum, uint? maximum)
        {
            if ((ulong)minimum * Memory.PageSize > uint.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(minimum), minimum, $"Maximum value allowed for memory minimum is {uint.MaxValue / Memory.PageSize}.");

            this.Minimum = minimum;
            this.Maximum = Math.Min(maximum.GetValueOrDefault(uint.MaxValue / Memory.PageSize), uint.MaxValue / Memory.PageSize);

            var size = checked(minimum * Memory.PageSize);

            if (size == 0)
                return;

            var start = this.Start = Marshal.AllocHGlobal(new IntPtr(size));
            this.Size = size;
            GC.AddMemoryPressure(size);

            var emptyPage = UnmanagedMemory.emptyPage.Reference;
            for (var i = 0; i < minimum; i++)
            {
                Marshal.Copy(
                    emptyPage,
                    0,
                    new IntPtr(start.ToInt64() + Memory.PageSize * i),
                    (int)Memory.PageSize / 8
                    );
            }
        }

        /// <summary>
        /// The initial size of memory in <see cref="Memory.PageSize"/> pages.
        /// </summary>
        public uint Minimum { get; }

        /// <summary>
        /// The maximum size of memory in <see cref="Memory.PageSize"/> pages.
        /// </summary>
        public uint Maximum { get; }

        /// <summary>
        /// The currently allocated size of memory in <see cref="Memory.PageSize"/> pages.
        /// </summary>
        public uint Current => this.Size / Memory.PageSize;

        /// <summary>
        /// The start of linear memory, or <see cref="IntPtr.Zero"/> if not used.
        /// </summary>
        public IntPtr Start { get; private set; }

        /// <summary>
        /// The current amount of memory allocated.
        /// </summary>
        public uint Size { get; private set; }

        /// <summary>
        /// Grows memory by <paramref name="delta"/> multiplied by <see cref="Memory.PageSize"/>.
        /// </summary>
        /// <param name="delta">The amount of memory pages to allocate.</param>
        /// <returns>The previous <see cref="Current"/> size value, or -1 (unchecked wrap to unsigned 32-bit integer) in the event of a failure.</returns>
        public uint Grow(uint delta)
        {
            var oldCurrent = this.Current;
            if (delta == 0)
                return oldCurrent;

            const uint failed = unchecked((uint)-1);
            try
            {
                if (checked(oldCurrent + delta) > this.Maximum)
                    return failed;

                var newCurrent = oldCurrent + delta;
                var newSize = newCurrent * Memory.PageSize;
                this.Start = Marshal.ReAllocHGlobal(this.Start, new IntPtr(newSize));
                GC.AddMemoryPressure(newSize - this.Size);
                this.Size = newSize;

                return oldCurrent;
            }
            catch
            {
            }

            return failed;
        }

        /// <summary>
        /// Calls <see cref="Dispose"/>.
        /// </summary>
        ~UnmanagedMemory() => this.Dispose();

        /// <summary>
        /// Releases unmanaged resources associated with this instance.
        /// </summary>
        public void Dispose()
        {
            if (this.Start == IntPtr.Zero)
                return;
            if (this.Size == 0)
                return;

            Marshal.FreeHGlobal(this.Start);
            GC.RemoveMemoryPressure(Memory.PageSize * (long)this.Size);
            this.Start = IntPtr.Zero;
            this.Size = 0;
            GC.SuppressFinalize(this);
        }
    }
}