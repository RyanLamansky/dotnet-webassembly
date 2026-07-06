using System;
using System.Collections.Generic;

namespace WebAssembly;

/// <summary>
/// A recursion group (WASM 3.0 garbage collection): one or more mutually-recursive composite types that share a
/// canonical identity boundary.  Recursion groups are an optional parallel view over <see cref="Module.Types"/>
/// (see <see cref="Module.RecursionGroups"/>) that records the on-the-wire grouping so it can be reproduced exactly.
/// </summary>
public sealed class RecursionGroup
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only",
        Justification = "Matches the mutable-collection convention used throughout the object model.")]
    private IList<WebAssemblyType>? types;

    /// <summary>
    /// The composite types belonging to this recursion group, in declaration order.
    /// </summary>
    /// <exception cref="ArgumentNullException">Value cannot be set to null.</exception>
    public IList<WebAssemblyType> Types
    {
        get => this.types ??= [];
        set => this.types = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// When true, this group is emitted with an explicit <c>0x4E</c> recursion-group marker; when false, a singleton
    /// group is emitted as a bare implicit group (the shorthand form).  Groups with zero or two-or-more members are
    /// always emitted explicitly regardless of this value.
    /// </summary>
    public bool IsExplicit { get; set; }

    /// <summary>
    /// Creates a new <see cref="RecursionGroup"/> instance.
    /// </summary>
    public RecursionGroup()
    {
    }
}
