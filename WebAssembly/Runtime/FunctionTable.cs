using System;
using System.Linq;
using System.Reflection;

namespace WebAssembly.Runtime;

/// <summary>
/// An array-like structure representing a table import/export, which for the initial specification of WebAssembly is always a function list.
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
        .GetDeclaredMethod(nameof(Grow))!
        );

    internal static readonly RegeneratingWeakReference<MethodInfo> InitFromSegmentMethod = new(() =>
        typeof(FunctionTable).GetTypeInfo().GetDeclaredMethod(nameof(InitFromSegment))!);

    internal static readonly RegeneratingWeakReference<MethodInfo> CopyMethod = new(() =>
        typeof(FunctionTable).GetTypeInfo().GetDeclaredMethod(nameof(Copy))!);

    internal static readonly RegeneratingWeakReference<MethodInfo> FillMethod = new(() =>
        typeof(FunctionTable).GetTypeInfo().GetDeclaredMethod(nameof(Fill))!);

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
    /// <param name="initValue">The value to initialize new table slots with.</param>
    /// <param name="number">The number of elements you want to grow the table by.</param>
    /// <returns>The previous length of the table, or uint.MaxValue if growth fails.</returns>
    public uint Grow(Delegate? initValue, uint number)
    {
        var oldSize = (uint)this.Length;
        
        // Check for overflow
        if (number > uint.MaxValue - oldSize)
            return uint.MaxValue; // Overflow would occur
            
        var newSize = oldSize + number;

        if (newSize > this.Maximum.GetValueOrDefault(uint.MaxValue))
            return uint.MaxValue; // WASM spec: return -1 (uint.MaxValue) on failure
        
        if (newSize > int.MaxValue)
            return uint.MaxValue; // Can't allocate array larger than int.MaxValue

        Array.Resize(ref delegates, (int)newSize);
        
        // Initialize new slots with initValue
        for (var i = oldSize; i < newSize; i++)
            delegates[i] = initValue;

        return oldSize;
    }

    /// <summary>
    /// Copies <paramref name="length"/> entries from <paramref name="src"/> starting at <paramref name="srcOffset"/> into this table at <paramref name="dst"/>.
    /// A null <paramref name="src"/> is treated as a dropped (length-0) segment.
    /// </summary>
    public void InitFromSegment(uint dst, Delegate?[]? src, uint srcOffset, uint length)
    {
        var srcLen = src != null ? (uint)src.Length : 0u;
        
        // Check for overflow
        if (length > uint.MaxValue - dst || length > uint.MaxValue - srcOffset)
            _ = this.delegates[int.MaxValue]; // throws IndexOutOfRangeException
            
        var dstEnd = dst + length;
        var srcEnd = srcOffset + length;
        // Trigger natural IndexOutOfRangeException on bounds violations (atomic pre-check).
        if (dstEnd > (uint)this.Length || srcEnd > srcLen)
            _ = this.delegates[int.MaxValue]; // throws IndexOutOfRangeException
        if (length == 0) return;
        for (var i = 0u; i < length; i++)
            this[(int)(dst + i)] = src![(int)(srcOffset + i)];
    }

    /// <summary>
    /// Copies <paramref name="length"/> entries from offset <paramref name="src"/> to offset <paramref name="dst"/> within this table.
    /// </summary>
    public void Copy(uint dst, uint src, uint length)
    {
        // Check for overflow
        if (length > uint.MaxValue - dst || length > uint.MaxValue - src)
            _ = this.delegates[int.MaxValue]; // throws IndexOutOfRangeException
            
        var dstEnd = dst + length;
        var srcEnd = src + length;
        if (dstEnd > (uint)this.Length || srcEnd > (uint)this.Length)
            _ = this.delegates[int.MaxValue]; // throws IndexOutOfRangeException
        if (length == 0) return;
        if (dst <= src)
            for (var i = 0u; i < length; i++)
                this[(int)(dst + i)] = this[(int)(src + i)];
        else
            for (var i = length; i > 0; i--)
                this[(int)(dst + i - 1)] = this[(int)(src + i - 1)];
    }

    /// <summary>
    /// Fills a range of the table with a delegate value.
    /// </summary>
    /// <param name="dst">Starting index to fill.</param>
    /// <param name="value">Delegate to fill with (may be null).</param>
    /// <param name="len">Number of elements to fill.</param>
    public void Fill(uint dst, Delegate? value, uint len)
    {
        // Check for overflow
        if (len > uint.MaxValue - dst)
            _ = this.delegates[int.MaxValue]; // throws IndexOutOfRangeException (out of bounds)
            
        var end = dst + len;
        if (end > (uint)this.Length)
            _ = this.delegates[int.MaxValue]; // throws IndexOutOfRangeException

        for (uint i = 0; i < len; i++)
            this[(int)(dst + i)] = value;
    }
}
