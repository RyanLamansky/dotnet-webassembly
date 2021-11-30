namespace WebAssembly;

/// <summary>
/// Many compiler tests can use this template to host the test.
/// </summary>
public abstract class CompilerTestBaseExportedMutableGlobal<T>
    where T : struct
{
    /// <summary>
    /// Creates a new <see cref="CompilerTestBaseExportedMutableGlobal{T}"/> instance.
    /// </summary>
    protected CompilerTestBaseExportedMutableGlobal()
    {
    }

    /// <summary>
    /// Returns a value.
    /// </summary>
    public abstract T Test { get; set; }
}
