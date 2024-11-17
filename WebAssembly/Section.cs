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
    None = 0,
    /// <summary>
    /// Function signature declarations.
    /// </summary>
    Type = 1,
    /// <summary>
    /// Import declarations.
    /// </summary>
    Import = 2,
    /// <summary>
    /// Function declarations.
    /// </summary>
    Function = 3,
    /// <summary>
    /// Indirect function table and other tables.
    /// </summary>
    Table = 4,
    /// <summary>
    /// Memory attributes.
    /// </summary>
    Memory = 5,
    /// <summary>
    /// Global declarations.
    /// </summary>
    Global = 6,
    /// <summary>
    /// Exports.
    /// </summary>
    Export = 7,
    /// <summary>
    /// Start function declaration.
    /// </summary>
    Start = 8,
    /// <summary>
    /// Elements section.
    /// </summary>
    Element = 9,
    /// <summary>
    /// Function bodies (code).
    /// </summary>
    Code = 10,
    /// <summary>
    /// Data segments.
    /// </summary>
    Data = 11,
    /// <summary>
    /// Data count section.
    /// </summary>
    DataCount = 12,
    /// <summary>
    /// Tag declaration.
    /// </summary>
    Tag = 13,
}

static class SectionExtensions
{
    public static bool IsValid(this Section section) => section >= Section.None && section <= Section.Tag;

    public static int GetSectionIndex(this Section section) => section switch
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
        _ => throw new ArgumentOutOfRangeException(nameof(section), section, null)
    };
}
