namespace WebAssembly;

/// <summary>
/// The encoded form of an <see cref="Element"/> segment (the leading flags value, 0–7). The low bit distinguishes
/// active (even) from passive/declarative (odd); a set bit 2 (values 4–7) selects per-element initializer
/// expressions over plain function indices.
/// </summary>
public enum ElementKind : uint
{
    /// <summary>Active, implicitly targeting table 0, with an offset expression and function indices.</summary>
    ActiveFunctionIndices = 0,

    /// <summary>Passive, with function indices.</summary>
    PassiveFunctionIndices = 1,

    /// <summary>Active, with an explicit table index, an offset expression, and function indices.</summary>
    ActiveExplicitTableFunctionIndices = 2,

    /// <summary>Declarative, with function indices.</summary>
    DeclarativeFunctionIndices = 3,

    /// <summary>Active, implicitly targeting table 0, with an offset expression and initializer expressions.</summary>
    ActiveExpressions = 4,

    /// <summary>Passive, with initializer expressions.</summary>
    PassiveExpressions = 5,

    /// <summary>Active, with an explicit table index, an offset expression, and initializer expressions.</summary>
    ActiveExplicitTableExpressions = 6,

    /// <summary>Declarative, with initializer expressions.</summary>
    DeclarativeExpressions = 7,
}
