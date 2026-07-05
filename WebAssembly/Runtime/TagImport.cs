namespace WebAssembly.Runtime;

/// <summary>
/// The base class for all exception-tag imports (WASM 3.0 exception handling). A tag is a unique reference-identity
/// object, one per (instance × tag definition), that gives thrown exceptions their identity and flows through linking
/// by reference. The only concrete kind today is <see cref="ExceptionTag"/>.
/// </summary>
public abstract class TagImport : RuntimeImport
{
    /// <summary>
    /// Always <see cref="ExternalKind.Tag"/>.
    /// </summary>
    public sealed override ExternalKind Kind => ExternalKind.Tag;

    // Set by RuntimeImport.FromCompiledExports when this tag is another compiled module's tag export: the closed
    // structural descriptor of its exception signature. Null for host-supplied tags; tag-import matching compares
    // descriptors for exact (invariant) equality, falling back to accept when either side is null.
    internal string? Descriptor;

    private protected TagImport()
    {
    }
}
