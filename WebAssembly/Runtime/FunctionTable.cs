using System;
using System.Linq;
using System.Reflection;

namespace WebAssembly.Runtime;

/// <summary>
/// An array-like structure representing a funcref table import/export (a list of functions).
/// WebAssembly 2.0 reference types added the externref counterpart, <see cref="ExternRefTable"/>.
/// Its behavior mimics the JavaScript version, where the import object is actually modified by the instantiation process.
/// </summary>
public class FunctionTable : TableImport
{
    internal static readonly RegeneratingWeakReference<MethodInfo> IndexGetter = new(() =>
        typeof(FunctionTable)
           .GetTypeInfo()
           .DeclaredProperties
           .First(prop => prop.GetIndexParameters().Length > 0)
           .GetMethod!
        );

    internal static readonly RegeneratingWeakReference<MethodInfo> IndexSetter = new(() =>
        typeof(FunctionTable)
           .GetTypeInfo()
           .DeclaredProperties
           .First(prop => prop.GetIndexParameters().Length > 0)
           .SetMethod!
        );

    internal static readonly RegeneratingWeakReference<MethodInfo> LengthGetter = new(() =>
        typeof(FunctionTable)
        .GetTypeInfo()
        .GetDeclaredProperty(nameof(Length))!
        .GetMethod!
        );

    internal static readonly RegeneratingWeakReference<MethodInfo> GrowMethod = new(() =>
        typeof(FunctionTable)
        .GetTypeInfo()
        .GetDeclaredMethods(nameof(Grow))
        .First(m => m.GetParameters().Length == 1)
        );

    internal static readonly RegeneratingWeakReference<MethodInfo> GrowWithValueMethod = new(() =>
        typeof(FunctionTable)
        .GetTypeInfo()
        .GetDeclaredMethods(nameof(Grow))
        .First(m => m.GetParameters().Length == 2)
        );

    internal static readonly RegeneratingWeakReference<MethodInfo> InitFromSegmentMethod = new(() =>
        typeof(FunctionTable).GetTypeInfo().GetDeclaredMethod(nameof(InitFromSegment))!);

    internal static readonly RegeneratingWeakReference<MethodInfo> FillMethod = new(() =>
        typeof(FunctionTable).GetTypeInfo().GetDeclaredMethod(nameof(Fill))!);

    internal static readonly RegeneratingWeakReference<MethodInfo> CopyBetweenMethod = new(() =>
        typeof(FunctionTable).GetTypeInfo().GetDeclaredMethods(nameof(Copy))
        .First(m => m.GetParameters().Length == 4));

    /// <summary>
    /// Always <see cref="ElementType.FunctionReference"/>.
    /// </summary>
    public sealed override ElementType Element => ElementType.FunctionReference;

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
    }

    private Delegate?[] delegates;

    /// <summary>
    /// Gets or sets the delegate at the indicated index.
    /// </summary>
    /// <param name="index">The index within the table to target.</param>
    /// <returns>The delegate at that index, which may be null.</returns>
    /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> does not fall within the range of the table.</exception>
    /// <remarks>
    /// Reference-type tables (WASM 2.0) move function references between arbitrary slots via table.copy/fill/init and
    /// <c>table.set</c>, so no per-slot delegate type is enforced; <c>call_indirect</c> verifies the type at call time.
    /// </remarks>
    public Delegate? this[int index]
    {
        get => this.delegates[index];
        set => this.delegates[index] = value;
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
    /// <paramref name="number"/>, when added to <see cref="Length"/>, would exceed the defined <see cref="Maximum"/>.
    /// </exception>
    /// <exception cref="OverflowException"><paramref name="number"/> added to the current size exceeds <see cref="int.MaxValue"/>.</exception>
    public uint Grow(uint number)
    {
        var oldSize = this.Length;
        var newSize = checked(oldSize + number);

        if (newSize > this.Maximum.GetValueOrDefault(uint.MaxValue))
            throw new ArgumentOutOfRangeException(nameof(number), $"{nameof(number)}, when added to {nameof(Length)}, would exceed the defined {nameof(Maximum)}.");

        var checkedSize = checked((int)newSize);
        Array.Resize(ref delegates, checkedSize);

        return oldSize;
    }

    /// <summary>
    /// Increases the size of the instance by a specified number of elements, initializing new slots with a value.
    /// Implements <c>table.grow</c>.
    /// </summary>
    /// <param name="initValue">The value to initialize new table slots with (may be null).</param>
    /// <param name="number">The number of elements to grow the table by.</param>
    /// <returns>The previous length of the table, or <see cref="uint.MaxValue"/> (-1) if growth fails.</returns>
    public uint Grow(Delegate? initValue, uint number)
    {
        var oldSize = this.Length;
        if (number > uint.MaxValue - oldSize)
            return uint.MaxValue;

        var newSize = oldSize + number;
        if (newSize > this.Maximum.GetValueOrDefault(uint.MaxValue) || newSize > int.MaxValue)
            return uint.MaxValue;

        Array.Resize(ref delegates, (int)newSize);
        for (var i = oldSize; i < newSize; i++)
            this.delegates[i] = initValue;

        return oldSize;
    }

    /// <summary>
    /// Copies <paramref name="length"/> entries from <paramref name="src"/> (starting at <paramref name="srcOffset"/>)
    /// into this table at <paramref name="dst"/>. Implements <c>table.init</c>. A null <paramref name="src"/> is treated
    /// as a dropped (length-0) segment.
    /// </summary>
    /// <exception cref="TableAccessOutOfRangeException">The source or destination range falls out of bounds.</exception>
    public void InitFromSegment(uint dst, Delegate?[]? src, uint srcOffset, uint length)
    {
        var srcLength = src != null ? (uint)src.Length : 0u;
        if ((ulong)dst + length > this.Length)
            throw new TableAccessOutOfRangeException(dst, length);
        if ((ulong)srcOffset + length > srcLength)
            throw new TableAccessOutOfRangeException(srcOffset, length);
        if (length == 0)
            return;
        for (var i = 0u; i < length; i++)
            this.delegates[dst + i] = src![srcOffset + i];
    }

    /// <summary>
    /// Copies <paramref name="length"/> entries from offset <paramref name="src"/> to offset <paramref name="dst"/>
    /// within this table, handling overlap. Implements <c>table.copy</c> with both indices targeting this table.
    /// </summary>
    /// <exception cref="TableAccessOutOfRangeException">The source or destination range falls out of bounds.</exception>
    public void Copy(uint dst, uint src, uint length)
    {
        if ((ulong)dst + length > this.Length)
            throw new TableAccessOutOfRangeException(dst, length);
        if ((ulong)src + length > this.Length)
            throw new TableAccessOutOfRangeException(src, length);
        if (length == 0)
            return;
        if (dst <= src)
            for (var i = 0u; i < length; i++)
                this.delegates[dst + i] = this.delegates[src + i];
        else
            for (var i = length; i > 0; i--)
                this.delegates[dst + i - 1] = this.delegates[src + i - 1];
    }

    /// <summary>
    /// Copies <paramref name="length"/> entries from <paramref name="srcTable"/> (offset <paramref name="srcIndex"/>)
    /// into this table at <paramref name="dstIndex"/>. Implements <c>table.copy</c> across two tables.
    /// </summary>
    /// <exception cref="TableAccessOutOfRangeException">The source or destination range falls out of bounds.</exception>
    public void Copy(FunctionTable srcTable, uint dstIndex, uint srcIndex, uint length)
    {
        if ((ulong)dstIndex + length > this.Length)
            throw new TableAccessOutOfRangeException(dstIndex, length);
        if ((ulong)srcIndex + length > srcTable.Length)
            throw new TableAccessOutOfRangeException(srcIndex, length);
        if (length == 0)
            return;
        if (ReferenceEquals(this, srcTable))
        {
            this.Copy(dstIndex, srcIndex, length);
            return;
        }
        for (var i = 0u; i < length; i++)
            this.delegates[dstIndex + i] = srcTable.delegates[srcIndex + i];
    }

    /// <summary>
    /// Fills <paramref name="length"/> entries starting at <paramref name="dst"/> with <paramref name="value"/>.
    /// Implements <c>table.fill</c>.
    /// </summary>
    /// <exception cref="TableAccessOutOfRangeException">The destination range falls out of bounds.</exception>
    public void Fill(uint dst, Delegate? value, uint length)
    {
        if ((ulong)dst + length > this.Length)
            throw new TableAccessOutOfRangeException(dst, length);
        for (var i = 0u; i < length; i++)
            this.delegates[dst + i] = value;
    }
}
