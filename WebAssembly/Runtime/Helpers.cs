using System;
using System.Collections.Generic;

namespace WebAssembly.Runtime;

/// <summary>
/// Helper logic for compiled WebAssembly modules.  Do not directly use these methods in your code.
/// </summary>
public static class Helpers
{
    /// <summary>
    /// Do not use this method directly in your code.
    /// </summary>
    [Obsolete("Do not use this method directly in your code.")]
    public static T FindImport<T>(IDictionary<string, IDictionary<string, RuntimeImport>> imports, string module, string field)
        where T : RuntimeImport
    {
#if NETSTANDARD
        if (imports == null)
            throw new ArgumentNullException(nameof(imports));
        if (module == null)
            throw new ArgumentNullException(nameof(module));
        if (field == null)
            throw new ArgumentNullException(nameof(field));
#else
        ArgumentNullException.ThrowIfNull(imports, nameof(imports));
        ArgumentNullException.ThrowIfNull(module, nameof(module));
        ArgumentNullException.ThrowIfNull(field, nameof(field));
#endif

        if (!imports.TryGetValue(module, out var fields) || !fields.TryGetValue(field, out var import) || import == null)
        {
            throw new ImportException($"Missing import for {module}::{field}.");
        }

        if (import is not T cast)
        {
            throw new ImportException($"Import for {module}::{field}. was not of the required type {typeof(T).Name}.");
        }

        return cast;
    }
}
