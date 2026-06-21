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

    private protected TableImport()
    {
    }
}
