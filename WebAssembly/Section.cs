﻿using System;

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
}
