using System;

namespace WebAssembly.Runtime;

/// <summary>
/// Indicates a memory source to use for a WebAssembly memory import.
/// </summary>
public class MemoryImport : RuntimeImport
{
    /// <summary>
    /// Always <see cref="ExternalKind.Function"/>.
    /// </summary>
    public sealed override ExternalKind Kind => ExternalKind.Memory;

    /// <summary>
    /// The method to use for the import.
    /// </summary>
    public Func<UnmanagedMemory> Method { get; private set; }

    /// <summary>
    /// Creates a new <see cref="MemoryImport"/> instance with the provided <see cref="UnmanagedMemory"/>.
    /// </summary>
    /// <param name="method">A function to provide the memory to use for the import.</param>
    /// <exception cref="ArgumentNullException">No parameters can be null.</exception>
    public MemoryImport(Func<UnmanagedMemory> method)
    {
        this.Method = method ?? throw new ArgumentNullException(nameof(method));
    }
}
