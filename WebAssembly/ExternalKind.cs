namespace WebAssembly
{
    /// <summary>
    /// A single-byte unsigned integer indicating the kind of definition being imported or defined.
    /// </summary>
    public enum ExternalKind : byte
    {
        /// <summary>
        /// A function import or definition.
        /// </summary>
        Function = 0,

        /// <summary>
        /// A table import or definition.
        /// </summary>
        Table = 1,

        /// <summary>
        /// A memory import or definition.
        /// </summary>
        Memory = 2,

        /// <summary>
        /// A global import or definition.
        /// </summary>
        Global = 3,
    }
}