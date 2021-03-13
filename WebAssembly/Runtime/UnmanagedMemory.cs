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
        internal static readonly RegeneratingWeakReference<MethodInfo> SizeGetter = new(()
            => typeof(UnmanagedMemory).GetTypeInfo().DeclaredProperties.Where(prop => prop.Name == nameof(Size)).First().GetMethod!);
        internal static readonly RegeneratingWeakReference<MethodInfo> StartGetter = new(()
            => typeof(UnmanagedMemory).GetTypeInfo().DeclaredProperties.Where(prop => prop.Name == nameof(Start)).First().GetMethod!);
        internal static readonly RegeneratingWeakReference<MethodInfo> GrowMethod = new(()
            => typeof(UnmanagedMemory).GetTypeInfo().DeclaredMethods.Where(prop => prop.Name == nameof(Grow)).First());

        private bool disposed;
        
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

            if (minimum == 0)
                return;

            this.Grow(minimum);
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
            if (this.disposed)
                throw new ObjectDisposedException(nameof(UnmanagedMemory));

            var oldCurrent = this.Current;
            if (delta == 0)
                return oldCurrent;

            static unsafe void ZeroMemory(IntPtr s, uint n)
            {
                // CIL `initblk` can't be generated from C# (as of v8.0).
                // Using run-time code generation here would interfere with AoT efforts.
                // The logic below is 95% as fast as `initblk`.
                var p = (ulong*)s;
                var limit = n / 8;
                var init = 0ul; // Not const for smaller CIL size and potentially more favorable JIT optimization.

                for (var i = 0; i < limit; i += 8)
                {
                    p[i] = init;
                    p[i + 1] = init;
                    p[i + 2] = init;
                    p[i + 3] = init;
                    p[i + 4] = init;
                    p[i + 5] = init;
                    p[i + 6] = init;
                    p[i + 7] = init;
                }
            }

            const uint failed = unchecked((uint)-1);
            try
            {
                if (checked(oldCurrent + delta) > this.Maximum)
                    return failed;

                var newCurrent = oldCurrent + delta;
                var newSize = newCurrent * Memory.PageSize;
                if (this.Start == default)
                {
                    this.Start = Marshal.AllocHGlobal(new IntPtr(newSize));
                    ZeroMemory(this.Start, newSize);
                }
                else
                {
                    this.Start = Marshal.ReAllocHGlobal(this.Start, new IntPtr(newSize));
                    ZeroMemory(this.Start + checked((int) this.Size), newSize - this.Size);
                }
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
            this.disposed = true;

            if (this.Start == IntPtr.Zero)
                return;
            if (this.Size == 0)
                return;

            Marshal.FreeHGlobal(this.Start);
            this.Start = IntPtr.Zero;
            this.Size = 0;
            GC.SuppressFinalize(this);
        }
    }
}