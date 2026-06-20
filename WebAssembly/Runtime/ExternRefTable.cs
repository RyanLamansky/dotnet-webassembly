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
        typeof(ExternRefTable).GetTypeInfo().GetDeclaredMethod(nameof(Fill))!);

    internal static readonly RegeneratingWeakReference<MethodInfo> CopyBetweenMethod = new(() =>
        typeof(ExternRefTable).GetTypeInfo().GetDeclaredMethods(nameof(Copy))
        .First(m => m.GetParameters().Length == 4));

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
    public uint Length => (uint)this.values.Length;

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
    /// <exception cref="IndexOutOfRangeException">The destination range falls out of bounds.</exception>
    public void Fill(uint dst, object? value, uint len)
    {
        if ((ulong)dst + len > (ulong)this.values.Length)
            _ = this.values[int.MaxValue]; // out of bounds: triggers IndexOutOfRangeException without CA2201
        for (uint i = 0; i < len; i++)
            this.values[dst + i] = value;
    }

    /// <summary>
    /// Copies <paramref name="len"/> entries from offset <paramref name="src"/> to offset <paramref name="dst"/>
    /// within this table. Implements <c>table.copy</c> with both indices targeting this table.
    /// </summary>
    /// <exception cref="IndexOutOfRangeException">The source or destination range falls out of bounds.</exception>
    public void Copy(uint dst, uint src, uint len)
    {
        if ((ulong)dst + len > (ulong)this.values.Length || (ulong)src + len > (ulong)this.values.Length)
            _ = this.values[int.MaxValue]; // out of bounds: triggers IndexOutOfRangeException without CA2201
        // Array.Copy handles overlapping ranges correctly.
        Array.Copy(this.values, (int)src, this.values, (int)dst, (int)len);
    }

    /// <summary>
    /// Copies <paramref name="len"/> entries from <paramref name="srcTable"/> (offset <paramref name="src"/>)
    /// into this table at <paramref name="dst"/>. Implements <c>table.copy</c> across two tables.
    /// </summary>
    /// <exception cref="IndexOutOfRangeException">The source or destination range falls out of bounds.</exception>
    public void Copy(ExternRefTable srcTable, uint dst, uint src, uint len)
    {
        if ((ulong)dst + len > (ulong)this.values.Length || (ulong)src + len > (ulong)srcTable.values.Length)
            _ = this.values[int.MaxValue]; // out of bounds: triggers IndexOutOfRangeException without CA2201
        Array.Copy(srcTable.values, (int)src, this.values, (int)dst, (int)len);
    }

    /// <summary>
    /// Copies <paramref name="length"/> entries from a passive element segment into this table.
    /// Implements <c>table.init</c>. A null <paramref name="src"/> is treated as a dropped (length-0) segment.
    /// </summary>
    /// <exception cref="IndexOutOfRangeException">The source or destination range falls out of bounds.</exception>
    public void InitFromSegment(uint dst, object?[]? src, uint srcOffset, uint length)
    {
        var srcLength = src != null ? (uint)src.Length : 0u;
        if ((ulong)dst + length > (ulong)this.values.Length || (ulong)srcOffset + length > srcLength)
            _ = this.values[int.MaxValue]; // out of bounds: triggers IndexOutOfRangeException without CA2201
        if (length == 0)
            return;
        for (var i = 0u; i < length; i++)
            this.values[dst + i] = src![srcOffset + i];
    }
}
