using System;

namespace WebAssembly;

/// <summary>
/// A WebAssembly 3.0 reference type: a nullable-or-not reference to either an abstract heap type
/// (<see cref="WebAssemblyHeapType"/>) or a concrete type index (<see cref="TypeIndex"/>).
/// </summary>
/// <remarks>
/// This is the long-form <c>(ref null? ht)</c> encoding (<c>0x63</c>/<c>0x64</c> followed by an s33 heap type).
/// The one-byte shorthands such as <see cref="WebAssemblyValueType.FuncRef"/> are represented by the plain
/// value-type enum, not by this struct, so that every pre-3.0 encoding round-trips byte-for-byte.
/// </remarks>
public readonly struct ReferenceType : IEquatable<ReferenceType>
{
    /// <summary>
    /// When true, a null value inhabits this type (the <c>0x63</c> form); otherwise the value is non-null (<c>0x64</c>).
    /// </summary>
    public bool IsNullable { get; }

    /// <summary>
    /// The abstract heap type this reference targets, or null when it targets a concrete <see cref="TypeIndex"/>.
    /// </summary>
    public WebAssemblyHeapType? HeapType { get; }

    /// <summary>
    /// The concrete type-section index this reference targets, or null when it targets an abstract <see cref="HeapType"/>.
    /// </summary>
    public uint? TypeIndex { get; }

    /// <summary>
    /// Creates a reference to an abstract heap type.
    /// </summary>
    /// <param name="heapType">The abstract heap type.</param>
    /// <param name="isNullable">Whether a null value inhabits this type.</param>
    public ReferenceType(WebAssemblyHeapType heapType, bool isNullable)
    {
        this.HeapType = heapType;
        this.TypeIndex = null;
        this.IsNullable = isNullable;
    }

    /// <summary>
    /// Creates a reference to a concrete type index.
    /// </summary>
    /// <param name="typeIndex">The type-section index.</param>
    /// <param name="isNullable">Whether a null value inhabits this type.</param>
    public ReferenceType(uint typeIndex, bool isNullable)
    {
        this.HeapType = null;
        this.TypeIndex = typeIndex;
        this.IsNullable = isNullable;
    }

    /// <summary>
    /// Reads a value type, returning the narrow enum value and, when it is a long-form reference type
    /// (<see cref="WebAssemblyValueType.Reference"/>/<see cref="WebAssemblyValueType.NullableReference"/>),
    /// the parsed <see cref="ReferenceType"/> in <paramref name="refType"/>.
    /// </summary>
    internal static WebAssemblyValueType ReadValueType(Reader reader, out ReferenceType? refType)
    {
        var value = (WebAssemblyValueType)reader.ReadVarInt7();
        if (value is WebAssemblyValueType.Reference or WebAssemblyValueType.NullableReference)
        {
            refType = Parse(reader, (sbyte)value);
            return value;
        }

        refType = null;
        return value;
    }

    /// <summary>
    /// Parses the heap type following an already-consumed <c>0x63</c>/<c>0x64</c> lead byte.
    /// </summary>
    internal static ReferenceType Parse(Reader reader, sbyte leadByte)
    {
        var isNullable = leadByte == (sbyte)WebAssemblyValueType.NullableReference;
        var heap = reader.ReadHeapTypeS33();
        if (heap >= 0)
            return new ReferenceType((uint)heap, isNullable);

        var heapType = (WebAssemblyHeapType)(sbyte)heap;
        if (!heapType.IsDefined())
            throw new ModuleLoadException($"Unrecognized heap type 0x{(byte)(sbyte)heap:X2}.", reader.Offset);

        return new ReferenceType(heapType, isNullable);
    }

    /// <summary>
    /// Emits the long-form reference-type encoding: the <c>0x63</c>/<c>0x64</c> lead byte followed by an s33 heap type.
    /// </summary>
    internal void WriteTo(Writer writer)
    {
        writer.WriteVar((int)(sbyte)(this.IsNullable
            ? WebAssemblyValueType.NullableReference
            : WebAssemblyValueType.Reference));
        writer.WriteHeapType(this.HeapType, this.TypeIndex);
    }

    /// <summary>
    /// The narrow value-type enum that represents this reference (<see cref="WebAssemblyValueType.NullableReference"/>
    /// when nullable, otherwise <see cref="WebAssemblyValueType.Reference"/>).
    /// </summary>
    internal WebAssemblyValueType ToValueType() => this.IsNullable
        ? WebAssemblyValueType.NullableReference
        : WebAssemblyValueType.Reference;

    /// <inheritdoc/>
    public bool Equals(ReferenceType other) =>
        this.IsNullable == other.IsNullable
        && this.HeapType == other.HeapType
        && this.TypeIndex == other.TypeIndex;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is ReferenceType other && this.Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => System.HashCode.Combine(this.IsNullable, this.HeapType, this.TypeIndex);

    /// <summary>Determines whether two <see cref="ReferenceType"/> values are equal.</summary>
    public static bool operator ==(ReferenceType left, ReferenceType right) => left.Equals(right);

    /// <summary>Determines whether two <see cref="ReferenceType"/> values are unequal.</summary>
    public static bool operator !=(ReferenceType left, ReferenceType right) => !left.Equals(right);

    /// <inheritdoc/>
    public override string ToString()
    {
        var target = this.HeapType is { } ht ? ht.ToString().ToLowerInvariant() : $"type {this.TypeIndex}";
        return this.IsNullable ? $"ref null {target}" : $"ref {target}";
    }
}
