using System;

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
}

static class SectionExtensions
{
    public static bool IsValid(this Section section) => section <= Section.DataCount;

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
            Section.Global => 6,
            Section.Export => 7,
            Section.Start => 8,
            Section.Element => 9,
            Section.DataCount => 10,
            Section.Code => 11,
            Section.Data => 12,
            _ => (int)section,
        };
    }
}
