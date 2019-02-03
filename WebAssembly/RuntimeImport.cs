using System;

namespace WebAssembly
{
    /// <summary>
    /// Functionality to integrate into a compiled WebAssembly instance.
    /// </summary>
    public abstract class RuntimeImport
    {
        /// <summary>
        /// The type of import.
        /// </summary>
        public abstract ExternalKind Kind { get; }

        /// <summary>
        /// The first portion of the two part name.
        /// </summary>
        public string ModuleName { get; private set; }

        /// <summary>
        /// The second portion of the two-part name.
        /// </summary>
        public string FieldName { get; private set; }

        private protected RuntimeImport(string moduleName, string exportName)
        {
            this.ModuleName = moduleName ?? throw new ArgumentNullException(nameof(moduleName));
            this.FieldName = exportName ?? throw new ArgumentNullException(nameof(exportName));
        }
    }
}