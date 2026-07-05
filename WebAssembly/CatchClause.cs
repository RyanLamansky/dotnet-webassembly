using System;

namespace WebAssembly;

/// <summary>
/// The kind of a <see cref="CatchClause"/> in a <see cref="Instructions.TryTable"/> (WASM 3.0 exception handling).
/// </summary>
public enum CatchKind : byte
{
    /// <summary>
    /// Catches an exception of a specific tag, branching with the tag's operands.
    /// </summary>
    Catch = 0,

    /// <summary>
    /// Catches an exception of a specific tag, branching with the tag's operands plus the exception reference.
    /// </summary>
    CatchRef = 1,

    /// <summary>
    /// Catches any WebAssembly exception, branching with no operands.
    /// </summary>
    CatchAll = 2,

    /// <summary>
    /// Catches any WebAssembly exception, branching with the exception reference.
    /// </summary>
    CatchAllRef = 3,
}

/// <summary>
/// A single handler clause of a <see cref="Instructions.TryTable"/>: when an exception matching this clause is caught,
/// control branches to <see cref="Label"/> carrying the appropriate operands (WASM 3.0 exception handling).
/// </summary>
public class CatchClause : IEquatable<CatchClause>
{
    /// <summary>
    /// The kind of this clause.
    /// </summary>
    public CatchKind Kind { get; set; }

    /// <summary>
    /// The tag index this clause matches. Meaningful only when <see cref="Kind"/> is
    /// <see cref="CatchKind.Catch"/> or <see cref="CatchKind.CatchRef"/>.
    /// </summary>
    public uint Tag { get; set; }

    /// <summary>
    /// The label (branch target, resolved in the context enclosing the <see cref="Instructions.TryTable"/>) to
    /// which control transfers when this clause matches.
    /// </summary>
    public uint Label { get; set; }

    /// <summary>
    /// Creates a new <see cref="CatchClause"/> instance.
    /// </summary>
    public CatchClause()
    {
    }

    /// <summary>
    /// Creates a new <see cref="CatchClause"/> instance with the provided parameters.
    /// </summary>
    /// <param name="kind">The kind of this clause.</param>
    /// <param name="label">The branch target label.</param>
    /// <param name="tag">The tag index (meaningful for <see cref="CatchKind.Catch"/>/<see cref="CatchKind.CatchRef"/>).</param>
    public CatchClause(CatchKind kind, uint label, uint tag = 0)
    {
        this.Kind = kind;
        this.Label = label;
        this.Tag = tag;
    }

    internal CatchClause(Reader reader)
    {
        this.Kind = (CatchKind)reader.ReadByte();
        switch (this.Kind)
        {
            case CatchKind.Catch:
            case CatchKind.CatchRef:
                this.Tag = reader.ReadVarUInt32();
                this.Label = reader.ReadVarUInt32();
                break;
            case CatchKind.CatchAll:
            case CatchKind.CatchAllRef:
                this.Label = reader.ReadVarUInt32();
                break;
            default:
                throw new ModuleLoadException($"Unrecognized catch clause kind 0x{(byte)this.Kind:X2}.", reader.Offset);
        }
    }

    internal void WriteTo(Writer writer)
    {
        writer.Write((byte)this.Kind);
        switch (this.Kind)
        {
            case CatchKind.Catch:
            case CatchKind.CatchRef:
                writer.WriteVar(this.Tag);
                writer.WriteVar(this.Label);
                break;
            default:
                writer.WriteVar(this.Label);
                break;
        }
    }

    /// <summary>
    /// Determines whether this clause carries the exception reference to the label (the <c>_ref</c> forms).
    /// </summary>
    internal bool IsRef => this.Kind is CatchKind.CatchRef or CatchKind.CatchAllRef;

    /// <summary>
    /// Determines whether this clause matches every WebAssembly exception (the <c>catch_all</c> forms).
    /// </summary>
    internal bool IsCatchAll => this.Kind is CatchKind.CatchAll or CatchKind.CatchAllRef;

    /// <summary>
    /// Expresses the value of this instance as a string.
    /// </summary>
    /// <returns>A string representation of this instance.</returns>
    public override string ToString() => this.IsCatchAll
        ? $"{this.Kind} {this.Label}"
        : $"{this.Kind} tag:{this.Tag} {this.Label}";

    /// <inheritdoc/>
    public bool Equals(CatchClause? other) =>
        other != null
        && other.Kind == this.Kind
        && other.Tag == this.Tag
        && other.Label == this.Label;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => this.Equals(obj as CatchClause);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine((int)this.Kind, (int)this.Tag, (int)this.Label);
}
