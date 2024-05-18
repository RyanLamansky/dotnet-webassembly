using System;

namespace WebAssembly.Runtime;

/// <summary>
/// Describes an error encountered during execution of a compiled WebAssembly.
/// </summary>
/// <param name="message">Becomes <see cref="Exception.Message"/>.</param>
public abstract class RuntimeException(string message) : Exception(message)
{
}
