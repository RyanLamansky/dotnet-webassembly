using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
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

        /// <summary>
        /// Sets a <paramref name="length"/> of bytes to zero at a <paramref name="destination"/> memory location.
        /// </summary>
        /// <param name="destination">The write target memory location.</param>
        /// <param name="length">The count of bytes to be copied.</param>
        delegate void ZeroMemoryDelegate(IntPtr destination, uint length);

        /// <summary>
        /// Sets a length of bytes to the given value at a destination memory location.
        /// See <see cref="ZeroMemoryDelegate"/> for details.
        /// </summary>
        static readonly ZeroMemoryDelegate ZeroMemory;
	
        static UnmanagedMemory()
        {
            const string dynamicModuleName = nameof(UnmanagedMemory) + "DynamicMethods";
            
            var module = AssemblyBuilder.DefineDynamicAssembly(
                        new AssemblyName(dynamicModuleName),
                        AssemblyBuilderAccess.RunAndCollect
                    )
                    .DefineDynamicModule(dynamicModuleName);

            var dynClass = module.DefineType(
                "DynamicMethods",
                TypeAttributes.Public | TypeAttributes.Abstract | TypeAttributes.Sealed
            );
            
            var initBlock = dynClass.DefineMethod(
                nameof(ZeroMemory),
                MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.Final,
                CallingConventions.Standard,
                typeof(void), new []{typeof(IntPtr), typeof(uint)});
            initBlock.SetImplementationFlags(MethodImplAttributes.AggressiveInlining);
            {
                var il = initBlock.GetILGenerator();
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Initblk);
                il.Emit(OpCodes.Ret);
            }
            
            var createdClass = dynClass.CreateTypeInfo();
            
            ZeroMemory = (ZeroMemoryDelegate)Delegate.CreateDelegate(typeof(ZeroMemoryDelegate), createdClass.GetDeclaredMethod(nameof(ZeroMemory)));
        }

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

            var size = checked(minimum * Memory.PageSize);

            if (size == 0)
                return;

            var start = this.Start = Marshal.AllocHGlobal(new IntPtr(size));
            this.Size = size;
            GC.AddMemoryPressure(size);
            
            ZeroMemory(this.Start, size);
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
            this.disposed = true;

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