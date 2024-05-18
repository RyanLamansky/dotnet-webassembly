using System;
using System.Reflection;
using System.Reflection.Emit;

namespace WebAssembly.Runtime.Compilation;

/// <summary>
/// Describes the raw, original export that was converted to a .NET instance member during compilation.
/// </summary>
/// <param name="kind">The original export type.</param>
/// <param name="name">Creates a new <see cref="NativeExportAttribute"/> from the provided parameters.</param>
[AttributeUsage(AttributeTargets.Property)]
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

    static readonly RegeneratingWeakReference<ConstructorInfo> Constructor = new(() =>
        typeof(NativeExportAttribute).GetConstructors()[0]
    );

    internal static CustomAttributeBuilder Emit(ExternalKind kind, string name) =>
        new(Constructor, [kind, name]);
}
