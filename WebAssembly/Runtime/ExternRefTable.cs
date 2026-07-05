using System;
using System.Linq;
using System.Reflection;

namespace WebAssembly.Runtime;

/// <summary>
/// An array-like structure representing an externref table import/export (WASM 2.0 reference types).
/// Holds external host references (any .NET <see cref="object"/>).
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
        typeof(ExternRefTable).GetTypeInfo().GetDeclaredMethod(nameof(Grow))!);

    internal static readonly RegeneratingWeakReference<MethodInfo> FillMethod = new(() =>
        typeof(ExternRefTable).GetTypeInfo().GetDeclaredMethods(nameof(Fill))
        .First(m => m.GetParameters()[0].ParameterType == typeof(uint)));

    internal static readonly RegeneratingWeakReference<MethodInfo> Fill64Method = new(() =>
        typeof(ExternRefTable).GetTypeInfo().GetDeclaredMethods(nameof(Fill))
        .First(m => m.GetParameters()[0].ParameterType == typeof(ulong)));

    internal static readonly RegeneratingWeakReference<MethodInfo> CopyBetweenMethod = new(() =>
        typeof(ExternRefTable).GetTypeInfo().GetDeclaredMethods(nameof(Copy))
        .First(m => m.GetParameters().Length == 4 && m.GetParameters()[1].ParameterType == typeof(uint)));

    internal static readonly RegeneratingWeakReference<MethodInfo> CopyBetween64Method = new(() =>
        typeof(ExternRefTable).GetTypeInfo().GetDeclaredMethods(nameof(Copy))
        .First(m => m.GetParameters().Length == 4 && m.GetParameters()[1].ParameterType == typeof(ulong)));

    internal static readonly RegeneratingWeakReference<MethodInfo> InitFromSegmentMethod = new(() =>
        typeof(ExternRefTable).GetTypeInfo().GetDeclaredMethods(nameof(InitFromSegment))
        .First(m => m.GetParameters()[0].ParameterType == typeof(uint)));

    internal static readonly RegeneratingWeakReference<MethodInfo> InitFromSegment64Method = new(() =>
        typeof(ExternRefTable).GetTypeInfo().GetDeclaredMethods(nameof(InitFromSegment))
        .First(m => m.GetParameters()[0].ParameterType == typeof(ulong)));

    internal static readonly RegeneratingWeakReference<MethodInfo> Grow64Method = new(() =>
        typeof(ExternRefTable).GetTypeInfo().GetDeclaredMethod(nameof(Grow64))!);

    internal static readonly RegeneratingWeakReference<MethodInfo> Get64Method = new(() =>
        typeof(ExternRefTable).GetTypeInfo().GetDeclaredMethod(nameof(Get64))!);

    internal static readonly RegeneratingWeakReference<MethodInfo> Set64Method = new(() =>
        typeof(ExternRefTable).GetTypeInfo().GetDeclaredMethod(nameof(Set64))!);

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
    public sealed override uint? Maximum { get; }

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
    public sealed override uint Length => (uint)this.values.Length;

    /// <summary>
    /// Grows the table by the indicated number of elements, filling new elements with the provided value.
    /// Implements <c>table.grow</c>.
    /// </summary>
    /// <param name="delta">The number of elements to add.</param>
    /// <param name="value">The value to initialize new elements with.</param>
    /// <returns>The previous size of the table, or <see cref="uint.MaxValue"/> (-1) if the grow operation failed.</returns>
    public uint Grow(uint delta, object? value)
    {
        var currentLength = (uint)this.values.Length;
        if (delta > uint.MaxValue - currentLength)
            return uint.MaxValue;

        var newLength = currentLength + delta;
        if (newLength > this.Maximum.GetValueOrDefault(uint.MaxValue) || newLength > int.MaxValue)
            return uint.MaxValue;

        var previousLength = (uint)this.values.Length;
        Array.Resize(ref this.values, (int)newLength);
        for (var i = previousLength; i < newLength; i++)
            this.values[i] = value;

        return previousLength;
    }

    /// <summary>
    /// Fills <paramref name="len"/> entries starting at <paramref name="dst"/> with <paramref name="value"/>.
    /// Implements <c>table.fill</c>.
    /// </summary>
    /// <exception cref="TableAccessOutOfRangeException">The destination range falls out of bounds.</exception>
    public void Fill(uint dst, object? value, uint len)
    {
        if ((ulong)dst + len > (ulong)this.values.Length)
            throw new TableAccessOutOfRangeException(dst, len);
        for (uint i = 0; i < len; i++)
            this.values[dst + i] = value;
    }

    /// <summary>
    /// Copies <paramref name="len"/> entries from offset <paramref name="src"/> to offset <paramref name="dst"/>
    /// within this table. Implements <c>table.copy</c> with both indices targeting this table.
    /// </summary>
    /// <exception cref="TableAccessOutOfRangeException">The source or destination range falls out of bounds.</exception>
    public void Copy(uint dst, uint src, uint len)
    {
        if ((ulong)dst + len > (ulong)this.values.Length)
            throw new TableAccessOutOfRangeException(dst, len);
        if ((ulong)src + len > (ulong)this.values.Length)
            throw new TableAccessOutOfRangeException(src, len);
        // Array.Copy handles overlapping ranges correctly.
        Array.Copy(this.values, (int)src, this.values, (int)dst, (int)len);
    }

    /// <summary>
    /// Copies <paramref name="len"/> entries from <paramref name="srcTable"/> (offset <paramref name="src"/>)
    /// into this table at <paramref name="dst"/>. Implements <c>table.copy</c> across two tables.
    /// </summary>
    /// <exception cref="TableAccessOutOfRangeException">The source or destination range falls out of bounds.</exception>
    public void Copy(ExternRefTable srcTable, uint dst, uint src, uint len)
    {
        if ((ulong)dst + len > (ulong)this.values.Length)
            throw new TableAccessOutOfRangeException(dst, len);
        if ((ulong)src + len > (ulong)srcTable.values.Length)
            throw new TableAccessOutOfRangeException(src, len);
        Array.Copy(srcTable.values, (int)src, this.values, (int)dst, (int)len);
    }

    /// <summary>
    /// Copies <paramref name="length"/> entries from a passive element segment into this table.
    /// Implements <c>table.init</c>. A null <paramref name="src"/> is treated as a dropped (length-0) segment.
    /// </summary>
    /// <exception cref="TableAccessOutOfRangeException">The source or destination range falls out of bounds.</exception>
    public void InitFromSegment(uint dst, object?[]? src, uint srcOffset, uint length)
    {
        var srcLength = src != null ? (uint)src.Length : 0u;
        if ((ulong)dst + length > (ulong)this.values.Length)
            throw new TableAccessOutOfRangeException(dst, length);
        if ((ulong)srcOffset + length > srcLength)
            throw new TableAccessOutOfRangeException(srcOffset, length);
        if (length == 0)
            return;
        for (var i = 0u; i < length; i++)
            this.values[dst + i] = src![srcOffset + i];
    }

    // A table is bounded to under 2^31 entries, so any i64 (table64) index or length at or beyond the current
    // length is out of range; these u64 counterparts validate at full width before narrowing to an array index,
    // so a huge i64 traps rather than wrapping into a valid slot.

    /// <summary>Reads element <paramref name="index"/> of a 64-bit (table64) table. Implements <c>table.get</c>.</summary>
    /// <param name="index">The element index.</param>
    /// <returns>The element at the index, which may be null.</returns>
    /// <exception cref="TableAccessOutOfRangeException"><paramref name="index"/> falls outside the table.</exception>
    public object? Get64(ulong index)
    {
        if (index >= (ulong)this.values.Length)
            throw new TableAccessOutOfRangeException(index, 1);
        return this.values[(int)index];
    }

    /// <summary>Writes element <paramref name="index"/> of a 64-bit (table64) table. Implements <c>table.set</c>.</summary>
    /// <param name="index">The element index.</param>
    /// <param name="value">The value to store.</param>
    /// <exception cref="TableAccessOutOfRangeException"><paramref name="index"/> falls outside the table.</exception>
    public void Set64(ulong index, object? value)
    {
        if (index >= (ulong)this.values.Length)
            throw new TableAccessOutOfRangeException(index, 1);
        this.values[(int)index] = value;
    }

    /// <summary>The table64 counterpart of <see cref="Grow(uint, object)"/>, taking a u64 delta and returning the
    /// previous length or -1 on failure.</summary>
    /// <param name="delta">The number of elements to add.</param>
    /// <param name="value">The value to initialize new elements with.</param>
    /// <returns>The previous size of the table, or -1 if the grow operation failed.</returns>
    public long Grow64(ulong delta, object? value)
    {
        if (delta > int.MaxValue)
            return -1;
        var result = this.Grow((uint)delta, value);
        return result == uint.MaxValue ? -1L : result;
    }

    /// <summary>The table64 counterpart of <see cref="Fill(uint, object, uint)"/>, taking a u64 destination and length.</summary>
    /// <param name="dst">The destination index.</param>
    /// <param name="value">The value to store.</param>
    /// <param name="len">The number of elements to fill.</param>
    /// <exception cref="TableAccessOutOfRangeException">The destination range falls out of bounds.</exception>
    public void Fill(ulong dst, object? value, ulong len)
    {
        if (dst + len > (ulong)this.values.Length)
            throw new TableAccessOutOfRangeException(dst, len);
        for (ulong i = 0; i < len; i++)
            this.values[(int)(dst + i)] = value;
    }

    /// <summary>The table64 counterpart of <see cref="Copy(ExternRefTable, uint, uint, uint)"/>, taking u64 indices
    /// and length. Either table may be 64-bit; a 32-bit side simply receives widened operands.</summary>
    /// <param name="srcTable">The table read from.</param>
    /// <param name="dst">The destination index within this table.</param>
    /// <param name="src">The source index within <paramref name="srcTable"/>.</param>
    /// <param name="len">The number of elements to copy.</param>
    /// <exception cref="TableAccessOutOfRangeException">The source or destination range falls out of bounds.</exception>
    public void Copy(ExternRefTable srcTable, ulong dst, ulong src, ulong len)
    {
        if (dst + len > (ulong)this.values.Length)
            throw new TableAccessOutOfRangeException(dst, len);
        if (src + len > (ulong)srcTable.values.Length)
            throw new TableAccessOutOfRangeException(src, len);
        Array.Copy(srcTable.values, (int)src, this.values, (int)dst, (int)len);
    }

    /// <summary>The table64 counterpart of <see cref="InitFromSegment(uint, object[], uint, uint)"/>, taking a
    /// u64 destination (the source segment index and length remain i32).</summary>
    /// <param name="dst">The destination index within the table.</param>
    /// <param name="src">The source element segment, or null if it has been dropped.</param>
    /// <param name="srcOffset">The index within the source segment.</param>
    /// <param name="length">The number of elements to copy.</param>
    /// <exception cref="TableAccessOutOfRangeException">The source or destination range falls out of bounds.</exception>
    public void InitFromSegment(ulong dst, object?[]? src, uint srcOffset, uint length)
    {
        var srcLength = src != null ? (uint)src.Length : 0u;
        if (dst + length > (ulong)this.values.Length)
            throw new TableAccessOutOfRangeException(dst, length);
        if ((ulong)srcOffset + length > srcLength)
            throw new TableAccessOutOfRangeException(srcOffset, length);
        if (length == 0)
            return;
        for (var i = 0u; i < length; i++)
            this.values[(int)dst + i] = src![srcOffset + i];
    }
}
