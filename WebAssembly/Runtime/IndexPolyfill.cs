#if NETSTANDARD2_0
namespace System;

/// <summary>
/// Provides support for the C# index-from-end operator on target frameworks that do not define <see cref="Index" />.
/// </summary>
public readonly struct Index : IEquatable<Index>
{
    readonly int value;

    /// <summary>
    /// Initializes a new instance of the <see cref="Index" /> struct.
    /// </summary>
    /// <param name="value">The index value.</param>
    /// <param name="fromEnd"><see langword="true" /> if the index is from the end; otherwise, <see langword="false" />.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="value" /> is negative.</exception>
    public Index(int value, bool fromEnd = false)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(nameof(value));

        this.value = fromEnd ? ~value : value;
    }

    /// <summary>
    /// Gets an index that points to the first element.
    /// </summary>
    public static Index Start => new(0);

    /// <summary>
    /// Gets an index that points past the last element.
    /// </summary>
    public static Index End => new(0, fromEnd: true);

    /// <summary>
    /// Gets the index value.
    /// </summary>
    public int Value => value < 0 ? ~value : value;

    /// <summary>
    /// Gets a value indicating whether the index is from the end.
    /// </summary>
    public bool IsFromEnd => value < 0;

    /// <summary>
    /// Creates an index from a start-based value.
    /// </summary>
    public static Index FromStart(int value) => new(value);

    /// <summary>
    /// Creates an index from an end-based value.
    /// </summary>
    public static Index FromEnd(int value) => new(value, fromEnd: true);

    /// <summary>
    /// Calculates the absolute offset for a collection of the specified length.
    /// </summary>
    /// <param name="length">The collection length.</param>
    /// <returns>The absolute offset.</returns>
    public int GetOffset(int length)
    {
        var offset = value;
        if (IsFromEnd)
            offset += length + 1;

        return offset;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Index other && Equals(other);

    /// <inheritdoc />
    public bool Equals(Index other) => value == other.value;

    /// <inheritdoc />
    public override int GetHashCode() => value;

    /// <inheritdoc />
    public override string ToString() => IsFromEnd ? "^" + Value : Value.ToString();

    /// <summary>
    /// Creates an <see cref="Index" /> from an integer.
    /// </summary>
    public static implicit operator Index(int value) => FromStart(value);

    /// <summary>
    /// Determines whether two indexes are equal.
    /// </summary>
    public static bool operator ==(Index left, Index right) => left.Equals(right);

    /// <summary>
    /// Determines whether two indexes are not equal.
    /// </summary>
    public static bool operator !=(Index left, Index right) => !(left == right);
}
#endif
