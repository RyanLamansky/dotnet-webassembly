using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace WebAssembly.Runtime;

/// <summary>
/// Implements a WebAssembly linear memory resource using unmanaged memory.
/// </summary>
public sealed class UnmanagedMemory : IDisposable
{
    internal static readonly RegeneratingWeakReference<MethodInfo> SizeGetter = new(()
        => typeof(UnmanagedMemory).GetTypeInfo().DeclaredProperties.First(prop => prop.Name == nameof(Size)).GetMethod!);
    internal static readonly RegeneratingWeakReference<MethodInfo> StartGetter = new(()
        => typeof(UnmanagedMemory).GetTypeInfo().DeclaredProperties.First(prop => prop.Name == nameof(Start)).GetMethod!);
    internal static readonly RegeneratingWeakReference<MethodInfo> GrowMethod = new(()
        => typeof(UnmanagedMemory).GetTypeInfo().DeclaredMethods.First(prop => prop.Name == nameof(Grow)));
    internal static readonly RegeneratingWeakReference<MethodInfo> Grow64Method = new(()
        => typeof(UnmanagedMemory).GetTypeInfo().DeclaredMethods.First(prop => prop.Name == nameof(Grow64)));
    internal static readonly RegeneratingWeakReference<MethodInfo> CopyMethod = new(()
        => typeof(UnmanagedMemory).GetTypeInfo().DeclaredMethods.First(m => m.Name == nameof(Copy) && m.GetParameters()[0].ParameterType == typeof(uint)));
    internal static readonly RegeneratingWeakReference<MethodInfo> Copy64Method = new(()
        => typeof(UnmanagedMemory).GetTypeInfo().DeclaredMethods.First(m => m.Name == nameof(Copy) && m.GetParameters()[0].ParameterType == typeof(ulong)));
    internal static readonly RegeneratingWeakReference<MethodInfo> CopyBetweenMethod = new(()
        => typeof(UnmanagedMemory).GetTypeInfo().DeclaredMethods.First(m => m.Name == nameof(CopyBetween) && m.GetParameters()[1].ParameterType == typeof(uint)));
    internal static readonly RegeneratingWeakReference<MethodInfo> CopyBetween64Method = new(()
        => typeof(UnmanagedMemory).GetTypeInfo().DeclaredMethods.First(m => m.Name == nameof(CopyBetween) && m.GetParameters()[1].ParameterType == typeof(ulong)));
    internal static readonly RegeneratingWeakReference<MethodInfo> FillMethod = new(()
        => typeof(UnmanagedMemory).GetTypeInfo().DeclaredMethods.First(m => m.Name == nameof(Fill) && m.GetParameters()[0].ParameterType == typeof(uint)));
    internal static readonly RegeneratingWeakReference<MethodInfo> Fill64Method = new(()
        => typeof(UnmanagedMemory).GetTypeInfo().DeclaredMethods.First(m => m.Name == nameof(Fill) && m.GetParameters()[0].ParameterType == typeof(ulong)));
    internal static readonly RegeneratingWeakReference<MethodInfo> InitFromSegmentMethod = new(()
        => typeof(UnmanagedMemory).GetTypeInfo().DeclaredMethods.First(m => m.Name == nameof(InitFromSegment) && m.GetParameters()[0].ParameterType == typeof(uint)));
    internal static readonly RegeneratingWeakReference<MethodInfo> InitFromSegment64Method = new(()
        => typeof(UnmanagedMemory).GetTypeInfo().DeclaredMethods.First(m => m.Name == nameof(InitFromSegment) && m.GetParameters()[0].ParameterType == typeof(ulong)));

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
    /// When true, this is a 64-bit (memory64) memory addressed with i64; when false, a 32-bit memory addressed
    /// with i32. An import declared with one address type is not satisfied by a memory of the other.
    /// </summary>
    public bool Is64Bit { get; set; }

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
        ObjectDisposedException.ThrowIf(this.disposed, this);

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
            unsafe
            {
                if (this.Start == default)
                {
                    // AllocZeroed hands back zero-filled pages, so no explicit clearing is needed.
                    this.Start = (IntPtr)NativeMemory.AllocZeroed(newSize);
                }
                else
                {
                    // Realloc preserves the existing bytes (C realloc semantics, like the ReAllocHGlobal it
                    // replaced); the grown tail is uninitialized and must be cleared. The old size is applied
                    // via byte* arithmetic (zero-extended) so it stays correct above 2 GiB.
                    this.Start = (IntPtr)NativeMemory.Realloc((void*)this.Start, newSize);
                    NativeMemory.Clear((byte*)this.Start + this.Size, newSize - this.Size);
                }
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
    /// Grows a 64-bit (memory64) memory by <paramref name="delta"/> pages, returning the previous size in pages, or
    /// -1 on failure. Implements <c>memory.grow</c> for a memory64. A delta beyond what the native buffer can hold
    /// (always the case above <see cref="uint.MaxValue"/> pages) fails cleanly rather than corrupting memory.
    /// </summary>
    /// <param name="delta">The number of pages to grow by.</param>
    /// <returns>The previous <see cref="Current"/> size in pages, or -1 on failure.</returns>
    public long Grow64(ulong delta)
    {
        // The backing buffer is bounded to under 4 GiB, so any delta beyond a uint's worth of pages is unallocatable.
        if (delta > uint.MaxValue)
            return -1;

        var result = this.Grow((uint)delta);
        return result == unchecked((uint)-1) ? -1L : result;
    }

    /// <summary>
    /// Copies <paramref name="length"/> bytes from <paramref name="src"/> to <paramref name="dst"/> within this memory,
    /// handling overlapping regions correctly. Implements <c>memory.copy</c>.
    /// </summary>
    /// <param name="dst">The destination byte offset.</param>
    /// <param name="src">The source byte offset.</param>
    /// <param name="length">The number of bytes to copy.</param>
    /// <exception cref="MemoryAccessOutOfRangeException">The source or destination range falls outside of memory.</exception>
    public unsafe void Copy(uint dst, uint src, uint length)
    {
        var dstEnd = checked(dst + length);
        var srcEnd = checked(src + length);
        if (dstEnd > this.Size)
            throw new MemoryAccessOutOfRangeException(dstEnd, this.Size);
        if (srcEnd > this.Size)
            throw new MemoryAccessOutOfRangeException(srcEnd, this.Size);
        if (length == 0)
            return;
        // byte* arithmetic zero-extends the uint offsets, staying correct above 2 GiB where a (int) cast would sign-extend.
        Buffer.MemoryCopy((byte*)this.Start + src, (byte*)this.Start + dst, length, length);
    }

    /// <summary>
    /// The memory64 counterpart of <see cref="Copy(uint, uint, uint)"/>: offsets and length are u64 and the range
    /// checks trap (never wrap) when a region exceeds this memory's byte size or overflows 64-bit arithmetic.
    /// </summary>
    /// <param name="dst">The destination byte offset.</param>
    /// <param name="src">The source byte offset.</param>
    /// <param name="length">The number of bytes to copy.</param>
    /// <exception cref="MemoryAccessOutOfRangeException">The source or destination range falls outside of memory.</exception>
    public unsafe void Copy(ulong dst, ulong src, ulong length)
    {
        var dstEnd = checked(dst + length);
        var srcEnd = checked(src + length);
        if (dstEnd > this.Size)
            throw new MemoryAccessOutOfRangeException(dst, length);
        if (srcEnd > this.Size)
            throw new MemoryAccessOutOfRangeException(src, length);
        if (length == 0)
            return;
        Buffer.MemoryCopy((byte*)this.Start + src, (byte*)this.Start + dst, length, length);
    }

    /// <summary>
    /// Copies <paramref name="length"/> bytes from <paramref name="source"/> at <paramref name="src"/> into
    /// <paramref name="destination"/> at <paramref name="dst"/>. Implements <c>memory.copy</c> across two
    /// (possibly distinct) WebAssembly 3.0 memories; the same instance may be passed for both.
    /// </summary>
    /// <param name="destination">The memory written to.</param>
    /// <param name="dst">The destination byte offset within <paramref name="destination"/>.</param>
    /// <param name="source">The memory read from.</param>
    /// <param name="src">The source byte offset within <paramref name="source"/>.</param>
    /// <param name="length">The number of bytes to copy.</param>
    /// <exception cref="MemoryAccessOutOfRangeException">The source or destination range falls outside of its memory.</exception>
    public static unsafe void CopyBetween(UnmanagedMemory destination, uint dst, UnmanagedMemory source, uint src, uint length)
    {
        var dstEnd = checked(dst + length);
        var srcEnd = checked(src + length);
        if (dstEnd > destination.Size)
            throw new MemoryAccessOutOfRangeException(dstEnd, destination.Size);
        if (srcEnd > source.Size)
            throw new MemoryAccessOutOfRangeException(srcEnd, source.Size);
        if (length == 0)
            return;
        // Buffer.MemoryCopy is memmove-like, so a same-memory overlapping copy is handled correctly too.
        Buffer.MemoryCopy((byte*)source.Start + src, (byte*)destination.Start + dst, length, length);
    }

    /// <summary>
    /// The memory64 counterpart of <see cref="CopyBetween(UnmanagedMemory, uint, UnmanagedMemory, uint, uint)"/>:
    /// offsets and length are u64 and the range checks trap (never wrap). Either memory may be 64-bit; a 32-bit
    /// side simply receives a widened offset that is still bounds-checked against its own size.
    /// </summary>
    /// <param name="destination">The memory written to.</param>
    /// <param name="dst">The destination byte offset within <paramref name="destination"/>.</param>
    /// <param name="source">The memory read from.</param>
    /// <param name="src">The source byte offset within <paramref name="source"/>.</param>
    /// <param name="length">The number of bytes to copy.</param>
    /// <exception cref="MemoryAccessOutOfRangeException">The source or destination range falls outside of its memory.</exception>
    public static unsafe void CopyBetween(UnmanagedMemory destination, ulong dst, UnmanagedMemory source, ulong src, ulong length)
    {
        var dstEnd = checked(dst + length);
        var srcEnd = checked(src + length);
        if (dstEnd > destination.Size)
            throw new MemoryAccessOutOfRangeException(dst, length);
        if (srcEnd > source.Size)
            throw new MemoryAccessOutOfRangeException(src, length);
        if (length == 0)
            return;
        Buffer.MemoryCopy((byte*)source.Start + src, (byte*)destination.Start + dst, length, length);
    }

    /// <summary>
    /// Copies <paramref name="length"/> bytes from <paramref name="src"/> starting at <paramref name="srcOffset"/> into this
    /// memory at <paramref name="dst"/>. Implements <c>memory.init</c>. A null <paramref name="src"/> is treated as a dropped
    /// (length-0) segment.
    /// </summary>
    /// <param name="dst">The destination byte offset within memory.</param>
    /// <param name="src">The source data segment, or null if it has been dropped.</param>
    /// <param name="srcOffset">The byte offset within the source segment.</param>
    /// <param name="length">The number of bytes to copy.</param>
    /// <exception cref="MemoryAccessOutOfRangeException">The source or destination range falls outside of its bounds.</exception>
    public unsafe void InitFromSegment(uint dst, byte[]? src, uint srcOffset, uint length)
    {
        // A dropped (null) segment is treated as length-0 for bounds checking purposes (WASM spec).
        var srcLength = src != null ? (uint)src.Length : 0u;
        var dstEnd = checked(dst + length);
        var srcEnd = checked(srcOffset + length);
        if (dstEnd > this.Size)
            throw new MemoryAccessOutOfRangeException(dstEnd, this.Size);
        if (srcEnd > srcLength)
            throw new MemoryAccessOutOfRangeException(srcEnd, srcLength);
        if (length == 0)
            return;
        fixed (byte* pSrc = src)
            Buffer.MemoryCopy(pSrc + srcOffset, (byte*)this.Start + dst, length, length);
    }

    /// <summary>
    /// The memory64 counterpart of <see cref="InitFromSegment(uint, byte[], uint, uint)"/>: the destination offset is
    /// u64 (the source segment index and length remain i32) and the destination range check traps (never wraps).
    /// </summary>
    /// <param name="dst">The destination byte offset within memory.</param>
    /// <param name="src">The source data segment, or null if it has been dropped.</param>
    /// <param name="srcOffset">The byte offset within the source segment.</param>
    /// <param name="length">The number of bytes to copy.</param>
    /// <exception cref="MemoryAccessOutOfRangeException">The source or destination range falls outside of its bounds.</exception>
    public unsafe void InitFromSegment(ulong dst, byte[]? src, uint srcOffset, uint length)
    {
        var srcLength = src != null ? (uint)src.Length : 0u;
        var dstEnd = checked(dst + length);
        var srcEnd = checked((ulong)srcOffset + length);
        if (dstEnd > this.Size)
            throw new MemoryAccessOutOfRangeException(dst, length);
        if (srcEnd > srcLength)
            throw new MemoryAccessOutOfRangeException(srcOffset, length);
        if (length == 0)
            return;
        fixed (byte* pSrc = src)
            Buffer.MemoryCopy(pSrc + srcOffset, (byte*)this.Start + dst, length, length);
    }

    /// <summary>
    /// Fills <paramref name="length"/> bytes starting at <paramref name="dst"/> with the low 8 bits of
    /// <paramref name="value"/>. Implements <c>memory.fill</c>.
    /// </summary>
    /// <param name="dst">The destination byte offset.</param>
    /// <param name="value">The fill value; only the low 8 bits are used.</param>
    /// <param name="length">The number of bytes to fill.</param>
    /// <exception cref="MemoryAccessOutOfRangeException">The destination range falls outside of memory.</exception>
    public unsafe void Fill(uint dst, uint value, uint length)
    {
        var dstEnd = checked(dst + length);
        if (dstEnd > this.Size)
            throw new MemoryAccessOutOfRangeException(dstEnd, this.Size);
        if (length == 0)
            return;
        NativeMemory.Fill((byte*)this.Start + dst, length, (byte)value);
    }

    /// <summary>
    /// The memory64 counterpart of <see cref="Fill(uint, uint, uint)"/>: the destination offset and length are u64
    /// and the destination range check traps (never wraps).
    /// </summary>
    /// <param name="dst">The destination byte offset.</param>
    /// <param name="value">The fill value; only the low 8 bits are used.</param>
    /// <param name="length">The number of bytes to fill.</param>
    /// <exception cref="MemoryAccessOutOfRangeException">The destination range falls outside of memory.</exception>
    public unsafe void Fill(ulong dst, uint value, ulong length)
    {
        var dstEnd = checked(dst + length);
        if (dstEnd > this.Size)
            throw new MemoryAccessOutOfRangeException(dst, length);
        if (length == 0)
            return;
        // The range check above bounds length by Size (always under 4 GiB), so the nuint narrowing is exact.
        NativeMemory.Fill((byte*)this.Start + dst, (nuint)length, (byte)value);
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

        unsafe
        {
            NativeMemory.Free((void*)this.Start);
        }
        this.Start = IntPtr.Zero;
        this.Size = 0;
        GC.SuppressFinalize(this);
    }
}
