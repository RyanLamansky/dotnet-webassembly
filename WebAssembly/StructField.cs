using System;

namespace WebAssembly;

/// <summary>
/// A single field of a <see cref="FunctionType.Struct"/> composite type, or the element type of a
/// <see cref="FunctionType.Array"/> composite type (WASM 3.0 garbage collection).
/// </summary>
public sealed class StructField : IEquatable<StructField>
{
    /// <summary>
    /// The storage kind of this field when it is not <see cref="Packed"/>.  For a long-form typed reference
    /// (<see cref="WebAssemblyValueType.Reference"/>/<see cref="WebAssemblyValueType.NullableReference"/>), the full
    /// heap detail is carried in <see cref="StorageRefType"/>.  Ignored when <see cref="Packed"/> is not
    /// <see cref="PackedType.None"/>.
    /// </summary>
    public WebAssemblyValueType StorageType { get; set; }

    /// <summary>
    /// The full WASM 3.0 reference type of this field when it is a long-form typed reference, otherwise null.
    /// </summary>
    public ReferenceType? StorageRefType { get; set; }

    /// <summary>
    /// The packed storage kind of this field (<c>i8</c>/<c>i16</c>), or <see cref="PackedType.None"/> when the field
    /// stores a regular <see cref="StorageType"/>.
    /// </summary>
    public PackedType Packed { get; set; }

    /// <summary>
    /// When true, this field may be written after construction (<c>struct.set</c>/<c>array.set</c>).
    /// </summary>
    public bool IsMutable { get; set; }

    /// <summary>
    /// Creates a new <see cref="StructField"/> instance.
    /// </summary>
    public StructField()
    {
    }

    /// <summary>
    /// Compares this instance for equality with another.
    /// </summary>
    /// <param name="other">The other instance to compare against.</param>
    /// <returns>True if the two instances have the same values, otherwise false.</returns>
    public bool Equals(StructField? other) =>
        other != null
        && this.Packed == other.Packed
        && this.IsMutable == other.IsMutable
        && this.StorageRefType == other.StorageRefType
        && (this.StorageRefType != null || this.Packed != PackedType.None || this.StorageType == other.StorageType);

    /// <summary>
    /// Compares this instance for equality with another.
    /// </summary>
    /// <param name="obj">The other instance to compare against.</param>
    /// <returns>True if the two instances have the same values, otherwise false.</returns>
    public override bool Equals(object? obj) => this.Equals(obj as StructField);

    /// <summary>
    /// Creates a hash code based on the values of this instance.
    /// </summary>
    /// <returns>The hash code.</returns>
    public override int GetHashCode()
    {
        var storage = this.Packed == PackedType.None && this.StorageRefType == null ? (int)this.StorageType : 0;
        return HashCode.Combine((int)this.Packed, this.IsMutable ? 1 : 0, this.StorageRefType?.GetHashCode() ?? 0, storage);
    }

    /// <summary>
    /// Expresses the value of this instance as a string.
    /// </summary>
    /// <returns>A string representation of this instance.</returns>
    public override string ToString()
    {
        var storage = this.Packed switch
        {
            PackedType.I8 => "i8",
            PackedType.I16 => "i16",
            _ => this.StorageRefType is { } rt ? rt.ToString() : this.StorageType.ToString(),
        };
        return this.IsMutable ? $"(mut {storage})" : storage;
    }
}
