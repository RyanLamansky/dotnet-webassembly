using System;
using System.Collections.Generic;

namespace WebAssembly.Runtime
{
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
            if (!imports.TryGetValue(module, out var fields) || !fields.TryGetValue(field, out var import) || import == null)
            {
                throw new ArgumentException($"Missing import for {module}::{field}.", "imports");
            }

            if (!(import is T cast))
            {
                throw new ArgumentException($"Import for {module}::{field}. was not of the required type {typeof(T).Name}.", "imports");
            }

            return cast;
        }
    }
}
