using System;
using System.Reflection;
using System.Reflection.Emit;

namespace WebAssembly.Runtime.Compilation;

/// <summary>
/// Describes the raw, original export that was converted to a .NET instance member during compilation.
/// </summary>
/// <param name="kind">The original export type.</param>
/// <param name="name">Creates a new <see cref="NativeExportAttribute"/> from the provided parameters.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]
public sealed class NativeExportAttribute(ExternalKind kind, string name) : Attribute
{
    /// <summary>
    /// The original export type.
    /// </summary>
    public ExternalKind Kind { get; } = kind;

    /// <summary>
    /// The original export name, which may be different from the .NET name if the name isn't C#-compatible.
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// For a reference-typed global or table export, a closed structural rendering of its WebAssembly type
    /// (for example <c>ref null func</c>), used to validate cross-module linking; <c>null</c> for every other export.
    /// </summary>
    public string? Descriptor { get; init; }

    static readonly RegeneratingWeakReference<ConstructorInfo> Constructor = new(() =>
        typeof(NativeExportAttribute).GetConstructors()[0]
    );

    static readonly RegeneratingWeakReference<PropertyInfo> DescriptorProperty = new(() =>
        typeof(NativeExportAttribute).GetProperty(nameof(Descriptor))!
    );

    internal static CustomAttributeBuilder Emit(ExternalKind kind, string name, string? descriptor = null) =>
        descriptor is null
            ? new(Constructor, [kind, name])
            : new(Constructor, [kind, name], [DescriptorProperty], [descriptor]);
}
