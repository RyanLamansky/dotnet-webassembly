using System;
using System.Reflection;
using System.Reflection.Emit;

namespace WebAssembly.Runtime.Compilation
{
    /// <summary>
    /// Describes the raw, original export that was converted to a .NET instance member during compilation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class NativeExportAttribute : Attribute
    {
        /// <summary>
        /// The original export type.
        /// </summary>
        public ExternalKind Kind { get; }

        /// <summary>
        /// The original export name, which may be different from the .NET name if the name isn't C#-compatible.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Creates a new <see cref="NativeExportAttribute"/> from the provided parameters.
        /// </summary>
        /// <param name="kind">The original export type.</param>
        /// <param name="name">Creates a new <see cref="NativeExportAttribute"/> from the provided parameters.</param>
        public NativeExportAttribute(ExternalKind kind, string name)
        {
            this.Kind = kind;
            this.Name = name;
        }

        static readonly RegeneratingWeakReference<ConstructorInfo> Constructor = new(() =>
            typeof(NativeExportAttribute).GetConstructors()[0]
        );

        internal static CustomAttributeBuilder Emit(ExternalKind kind, string name) =>
            new(Constructor, new object[] { kind, name });
    }
}
