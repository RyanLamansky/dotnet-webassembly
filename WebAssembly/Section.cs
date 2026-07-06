namespace WebAssembly;

/// <summary>
/// The standard section identifiers.
/// </summary>
public enum Section : byte
{
    /// <summary>
    /// Indicates a non-standard custom section.
    /// </summary>
    None,
    /// <summary>
    /// Function signature declarations.
    /// </summary>
    Type,
    /// <summary>
    /// Import declarations.
    /// </summary>
    Import,
    /// <summary>
    /// Function declarations.
    /// </summary>
    Function,
    /// <summary>
    /// Indirect function table and other tables.
    /// </summary>
    Table,
    /// <summary>
    /// Memory attributes.
    /// </summary>
    Memory,
    /// <summary>
    /// Global declarations.
    /// </summary>
    Global,
    /// <summary>
    /// Exports.
    /// </summary>
    Export,
    /// <summary>
    /// Start function declaration.
    /// </summary>
    Start,
    /// <summary>
    /// Elements section.
    /// </summary>
    Element,
    /// <summary>
    /// Function bodies (code).
    /// </summary>
    Code,
    /// <summary>
    /// Data segments.
    /// </summary>
    Data,
    /// <summary>
    /// Optional segment which indicates the number of sata segments
    /// </summary>
    DataCount,
    /// <summary>
    /// Exception tag declarations (WASM 3.0 exception handling). Positioned between the Memory and Global sections
    /// in the binary layout despite its higher numeric ID.
    /// </summary>
    Tag = 13,
}

static class SectionExtensions
{
    public static bool IsValid(this Section section) => section <= Section.Tag;

    extension(Section section)
    {
        /// <summary>
        /// The position of the section within the binary module layout, which is not the same as the
        /// numeric section ID: <see cref="Section.DataCount"/> (ID 12) must appear between
        /// <see cref="Section.Element"/> and <see cref="Section.Code"/>, not after <see cref="Section.Data"/>.
        /// Used to enforce section ordering during reading.
        /// </summary>
        public int BinaryOrder => section switch
        {
            Section.None => 0,
            Section.Type => 1,
            Section.Import => 2,
            Section.Function => 3,
            Section.Table => 4,
            Section.Memory => 5,
            Section.Tag => 6,
            Section.Global => 7,
            Section.Export => 8,
            Section.Start => 9,
            Section.Element => 10,
            Section.DataCount => 11,
            Section.Code => 12,
            Section.Data => 13,
            _ => (int)section,
        };
    }
}
