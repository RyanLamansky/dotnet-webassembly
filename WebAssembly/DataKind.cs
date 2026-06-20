namespace WebAssembly;

/// <summary>
/// The encoded form of a <see cref="Data"/> segment (the leading flags value, 0–2).
/// </summary>
public enum DataKind : uint
{
    /// <summary>Active, implicitly targeting memory 0, with an offset expression and bytes.</summary>
    Active = 0,

    /// <summary>Passive: no memory and no offset, just bytes held for <c>memory.init</c>.</summary>
    Passive = 1,

    /// <summary>Active, with an explicit memory index, an offset expression, and bytes.</summary>
    ActiveExplicitMemory = 2,
}
