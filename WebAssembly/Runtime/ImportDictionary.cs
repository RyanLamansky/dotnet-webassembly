using System.Collections.Generic;

namespace WebAssembly.Runtime
{
    /// <summary>
    /// A helper dictionary to make it easier to gather imports for the compiler.
    /// </summary>
    public class ImportDictionary : Dictionary<string, IDictionary<string, RuntimeImport>>,  IDictionary<string, IDictionary<string, RuntimeImport>>
    {
        /// <summary>
        /// Adds a runtime import with the provided module and field names.
        /// </summary>
        /// <param name="moduleName">The first part of the two-part name.</param>
        /// <param name="fieldName">The second part of the two-part name.</param>
        /// <param name="value">The import to add.</param>
        public void Add(string moduleName, string fieldName, RuntimeImport value)
        {
            if (!TryGetValue(moduleName, out var modules))
            {
                Add(moduleName, modules = new Dictionary<string, RuntimeImport>());
            }

            modules.Add(fieldName, value);
        }
    }
}
