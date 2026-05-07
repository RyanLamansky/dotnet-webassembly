using System;
using System.Linq;
using System.Reflection;

namespace WebAssembly.Runtime;

/// <summary>
/// An array-like structure representing an externref table import/export.
/// Holds external host references (any .NET object).
/// </summary>
public class ExternRefTable : TableImport
{
    internal static readonly RegeneratingWeakReference<MethodInfo> IndexGetter = new(() =>
        typeof(ExternRefTable)
           .GetTypeInfo()
           .DeclaredProperties
           .First(prop => prop.GetIndexParameters().Length > 0)
           .GetMethod!
        );

    internal static readonly RegeneratingWeakReference<MethodInfo> IndexSetter = new(() =>
        typeof(ExternRefTable)
           .GetTypeInfo()
           .DeclaredProperties
           .First(prop => prop.GetIndexParameters().Length > 0)
           .SetMethod!
        );

    internal static readonly RegeneratingWeakReference<MethodInfo> LengthGetter = new(() =>
        typeof(ExternRefTable)
        .GetTypeInfo()
        .GetDeclaredProperty(nameof(Length))!
        .GetMethod!
        );

    internal static readonly RegeneratingWeakReference<MethodInfo> GrowMethod = new(() =>
        typeof(ExternRefTable)
        .GetTypeInfo()
        .GetDeclaredMethod(nameof(Grow))!
        );

    internal static readonly RegeneratingWeakReference<MethodInfo> FillMethod = new(() =>
        typeof(ExternRefTable).GetTypeInfo().GetDeclaredMethod(nameof(Fill))!);

    internal static readonly RegeneratingWeakReference<MethodInfo> CopyMethod = new(() =>
        typeof(ExternRefTable).GetTypeInfo().GetDeclaredMethod(nameof(Copy))!);

    internal static readonly RegeneratingWeakReference<MethodInfo> InitFromSegmentMethod = new(() =>
        typeof(ExternRefTable).GetTypeInfo().GetDeclaredMethod(nameof(InitFromSegment))!);

    /// <summary>
    /// Always <see cref="ElementType.ExternRef"/>.
    /// </summary>
    public sealed override ElementType Element => ElementType.ExternRef;

    /// <summary>
    /// The initial number of elements.
    /// </summary>
    public uint Initial { get; }

    /// <summary>
    /// The maximum number of elements the table is allowed to grow to.
    /// </summary>
    public uint? Maximum { get; }

    /// <summary>
    /// Creates a new <see cref="ExternRefTable"/> with the provided initial size with no maximum.
    /// </summary>
    /// <param name="initial">The initial number of elements.</param>
    public ExternRefTable(uint initial)
        : this(initial, null)
    {
    }

    /// <summary>
    /// Creates a new <see cref="ExternRefTable"/> with the provided initial and maximum size.
    /// </summary>
    /// <param name="initial">The initial number of elements.</param>
    /// <param name="maximum">The maximum number of elements the table is allowed to grow to.</param>
    /// <exception cref="ArgumentException"><paramref name="initial"/> cannot exceed <paramref name="maximum"/>.</exception>
    public ExternRefTable(uint initial, uint? maximum)
    {
        if (initial > maximum.GetValueOrDefault(uint.MaxValue))
            throw new ArgumentException("initial cannot exceed maximum.", nameof(initial));

        this.Initial = initial;
        this.Maximum = maximum;
        this.values = new object?[initial];
    }

    private object?[] values;

    /// <summary>
    /// Gets or sets the external reference at the indicated index.
    /// </summary>
    /// <param name="index">The index within the table to target.</param>
    /// <returns>The value at that index, which may be null.</returns>
    /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> does not fall within the range of the table.</exception>
    public object? this[int index]
    {
        get => this.values[index];
        set => this.values[index] = value;
    }

    /// <summary>
    /// The current length of the table.
    /// </summary>
    public int Length => this.values.Length;

    /// <summary>
    /// Grows the table by the indicated number of elements, filling new elements with the provided value.
    /// </summary>
    /// <param name="delta">The number of elements to add.</param>
    /// <param name="value">The value to initialize new elements with.</param>
    /// <returns>The previous size of the table, or -1 if the grow operation failed.</returns>
    public int Grow(uint delta, object? value)
    {
        var currentLength = (uint)this.values.Length;
        var newLength = currentLength + delta;

        if (newLength > this.Maximum.GetValueOrDefault(uint.MaxValue))
            return -1;

        if (newLength > int.MaxValue)
            return -1;

        var previousLength = this.values.Length;
        Array.Resize(ref this.values, (int)newLength);

        // Fill new entries with the provided value
        for (var i = previousLength; i < this.values.Length; i++)
            this.values[i] = value;

        return previousLength;
    }

    /// <summary>
    /// Fills a range of the table with a value.
    /// </summary>
    /// <param name="dst">Starting index to fill.</param>
    /// <param name="value">Value to fill with.</param>
    /// <param name="len">Number of elements to fill.</param>
    public void Fill(uint dst, object? value, uint len)
    {
        if ((ulong)dst + (ulong)len > (ulong)this.values.Length)
            _ = this.values[int.MaxValue]; // throw IndexOutOfRangeException for out-of-bounds

        for (uint i = 0; i < len; i++)
            this.values[dst + i] = value;
    }

    /// <summary>
    /// Copies elements within or between tables.
    /// </summary>
    /// <param name="dst">Destination start index.</param>
    /// <param name="src">Source start index.</param>
    /// <param name="len">Number of elements to copy.</param>
    public void Copy(uint dst, uint src, uint len)
    {
        if ((ulong)dst + (ulong)len > (ulong)this.values.Length)
            _ = this.values[int.MaxValue]; // throw IndexOutOfRangeException

        if ((ulong)src + (ulong)len > (ulong)this.values.Length)
            _ = this.values[int.MaxValue]; // throw IndexOutOfRangeException

        Array.Copy(this.values, (int)src, this.values, (int)dst, (int)len);
    }

    /// <summary>
    /// Copies entries from a passive element segment into this table.
    /// </summary>
    /// <param name="dst">Destination index in the table.</param>
    /// <param name="src">Segment source (object?[] or null for dropped segments).</param>
    /// <param name="srcOffset">Offset within the source segment.</param>
    /// <param name="length">Number of elements to copy.</param>
    public void InitFromSegment(uint dst, object?[]? src, uint srcOffset, uint length)
    {
        var srcLen = src != null ? (uint)src.Length : 0u;
        
        // Check for overflow using ulong arithmetic
        if ((ulong)dst + (ulong)length > (ulong)this.values.Length)
            _ = this.values[int.MaxValue]; // throw IndexOutOfRangeException
        if ((ulong)srcOffset + (ulong)length > (ulong)srcLen)
            _ = this.values[int.MaxValue]; // throw IndexOutOfRangeException
            
        if (length == 0) return;
        
        for (var i = 0u; i < length; i++)
            this[(int)(dst + i)] = src![(int)(srcOffset + i)];
    }
}
