namespace WebAssembly.Runtime;

/// <summary>
/// The base class for all table imports: a <see cref="FunctionTable"/> (funcref) or, as of WebAssembly 2.0
/// reference types, an <see cref="ExternRefTable"/> (externref).
/// </summary>
public abstract class TableImport : RuntimeImport
{
    /// <summary>
    /// Always <see cref="ExternalKind.Table"/>.
    /// </summary>
    public sealed override ExternalKind Kind => ExternalKind.Table;

    /// <summary>
    /// The type of table being imported.
    /// </summary>
    public abstract ElementType Element { get; }

    /// <summary>
    /// The current number of entries the table holds.
    /// </summary>
    public abstract uint Length { get; }

    /// <summary>
    /// The maximum number of entries the table may hold, or null when unbounded.
    /// </summary>
    public abstract uint? Maximum { get; }

    /// <summary>
    /// When true, this is a 64-bit (table64) table indexed with i64; when false, a 32-bit table indexed with i32.
    /// An import declared with one address type is not satisfied by a table of the other.
    /// </summary>
    public bool Is64Bit { get; set; }

    private protected TableImport()
    {
    }
}
