using System;

namespace WebAssembly;

/// <summary>
/// Each local entry declares a number of local variables of a given type.
/// </summary>
public class Local : IEquatable<Local>
{
    /// <summary>
    ///  The number of local variables of <see cref="Type"/>.
    /// </summary>
    public uint Count { get; set; }

    private WebAssemblyValueType type;
    private ReferenceType? refType;

    /// <summary>
    /// Type of the variables. For a WASM 3.0 long-form typed reference this reports
    /// <see cref="WebAssemblyValueType.Reference"/>/<see cref="WebAssemblyValueType.NullableReference"/>;
    /// the heap-type detail is available via <see cref="RefType"/>. Setting this clears <see cref="RefType"/>.
    /// </summary>
    public WebAssemblyValueType Type
    {
        get => this.refType?.ToValueType() ?? this.type;
        set { this.type = value; this.refType = null; }
    }

    /// <summary>
    /// The full WASM 3.0 reference type when the local's type is a long-form typed reference, otherwise null.
    /// Setting this to a non-null value makes <see cref="Type"/> report the corresponding reference kind.
    /// </summary>
    public ReferenceType? RefType
    {
        get => this.refType;
        set => this.refType = value;
    }

    /// <summary>
    /// Creates a new <see cref="Local"/> instance.
    /// </summary>
    public Local()
    {
    }

    internal Local(Reader reader)
    {
        this.Count = reader.ReadVarUInt32();
        this.type = ReferenceType.ReadValueType(reader, out this.refType);
    }

    /// <summary>
    /// Expresses the value of this instance as a string.
    /// </summary>
    /// <returns>A string representation of this instance.</returns>
    public override string ToString() => $"{Count} of {Type}";

    /// <summary>
    /// Returns a hash code based on the value of this instance.
    /// </summary>
    /// <returns>The hash code.</returns>
    public override int GetHashCode() => HashCode.Combine((int)this.Type, (int)this.Count, this.refType?.GetHashCode() ?? 0);

    /// <summary>
    /// Determines whether this instance is identical to another.
    /// </summary>
    /// <param name="obj">The object instance to compare against.</param>
    /// <returns>True if they have the same type and value, otherwise false.</returns>
    public override bool Equals(object? obj) => this.Equals(obj as Local);

    /// <summary>
    /// Determines whether this instance is identical to another.
    /// </summary>
    /// <param name="other">The instance to compare against.</param>
    /// <returns>True if they have the same type and value, otherwise false.</returns>
    public bool Equals(Local? other)
    {
        return other != null
            && other.Type == this.Type
            && other.refType == this.refType
            && other.Count == this.Count;
    }

    internal void WriteTo(Writer writer)
    {
        writer.WriteVar(this.Count);
        if (this.refType is { } rt)
            rt.WriteTo(writer);
        else
            writer.WriteVar((sbyte)this.type);
    }
}
