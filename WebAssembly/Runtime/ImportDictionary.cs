using System;
using System.Collections.Generic;

namespace WebAssembly.Runtime
{
    /// <summary>
    /// No members of its own, essentially an alias for a dictionary of dictionaries,
    /// both keyed by strings, with the inner dictionary having values of <see cref="RuntimeImport"/>.
    /// </summary>
    public class ImportDictionary : Dictionary<string, IDictionary<string, RuntimeImport>>, IDictionary<string, IDictionary<string, RuntimeImport>>
    {
        /// <summary>
        /// Creates a new <see cref="ImportDictionary"/> instance.
        /// </summary>
        public ImportDictionary()
        {
        }
    }

    /// <summary>
    /// Provides helper functions to dictionaries compatible with compilation.
    /// </summary>
    public static class ImportDictionaryExtensions
    {
        /// <summary>
        /// Adds a runtime import with the provided module and field names.
        /// </summary>
        /// <param name="dictionary">The dictionary instance to receive the values.</param>
        /// <param name="moduleName">The first part of the two-part name.</param>
        /// <param name="fieldName">The second part of the two-part name.</param>
        /// <param name="value">The import to add.</param>
        public static void Add(
            this IDictionary<string, IDictionary<string, RuntimeImport>> dictionary,
            string moduleName,
            string fieldName,
            RuntimeImport value)
        {
            if (dictionary == null)
                throw new ArgumentNullException(nameof(dictionary));
            if (moduleName == null)
                throw new ArgumentNullException(nameof(moduleName));
            if (fieldName == null)
                throw new ArgumentNullException(nameof(fieldName));

            if (!dictionary.TryGetValue(moduleName, out var modules))
            {
                dictionary.Add(moduleName, modules = new Dictionary<string, RuntimeImport>());
            }

            modules.Add(fieldName, value);
        }

        /// <summary>
        /// Adds exports from a compiled instance to a compatible import dictionary.
        /// </summary>
        /// <param name="dictionary">The dictionary instance to receive the values.</param>
        /// <param name="moduleName">The first part of the two-part name; the second part comes from the export.</param>
        /// <param name="exports">The source of imports.</param>
        public static void AddFromExports<TExports>(
            this IDictionary<string, IDictionary<string, RuntimeImport>> dictionary,
            string moduleName,
            TExports exports)
        where TExports : class
        {
            if (dictionary == null)
                throw new ArgumentNullException(nameof(dictionary));
            if (moduleName == null)
                throw new ArgumentNullException(nameof(moduleName));
            if (exports == null)
                throw new ArgumentNullException(nameof(exports));

            foreach (var (name, import) in RuntimeImport.FromCompiledExports(exports))
            {
                dictionary.Add(moduleName, name, import);
            }
        }
    }
}
