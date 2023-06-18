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
    /// A count of the data segments used for single-pass validation of data references.s
    /// </summary>
    DataCount,
}

static class SectionExtensions
{
    public static bool IsValid(this Section section) => section >= Section.None && section <= Section.Data;

    public static bool IsInOrder(this Section section, Section previousSection)
    {
        if (section == Section.None)
            return true;

        if (section > previousSection)
            return true;

        if (previousSection != Section.DataCount)
            return false; // This is (currently) the only section type using non-numeric order.

        if (section is not Section.Code or Section.Data)
            return false; // These are (currently) the only section types allowed to follow DataCount.

        return true;
    }
}
