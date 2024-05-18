using System;

namespace WebAssembly;

/// <summary>
/// Describes the characteristics of an <see cref="OpCode"/>.
/// </summary>
/// <param name="name">The standardized name for the opcode.  Cannot be null.</param>
/// <exception cref="ArgumentNullException"><paramref name="name"/> cannot be null.</exception>
[AttributeUsage(AttributeTargets.Field)]
public sealed class OpCodeCharacteristicsAttribute(string name) : Attribute
{
    /// <summary>
    /// The standardized name for the opcode.  Cannot be null.
    /// </summary>
    public string Name { get; } = name ?? throw new ArgumentNullException(nameof(name));
}
