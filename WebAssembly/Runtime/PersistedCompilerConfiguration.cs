#if NET9_0_OR_GREATER

using System;
using System.Linq;
using System.Reflection;

namespace WebAssembly.Runtime;

/// <summary>
/// Extended compiler configurations for generation of persisted assemblies.
/// </summary>
public class PersistedCompilerConfiguration : CompilerConfiguration
{
    /// <summary>
    /// Creates a new <see cref="PersistedCompilerConfiguration"/> with the provided values.
    /// </summary>
    /// <param name="coreAssembly">A reference to a .NET Standard or .NET core DLL.</param>
    /// <param name="webAssembly">A reference to a WebAssembly for .NET primary DLL linked to <paramref name="coreAssembly"/>.</param>
    /// <param name="outputName">The name that will be given to the output.</param>
    /// <param name="moduleName">The name that will be given to the module within the output assembly.</param>
    /// <exception cref="ArgumentNullException">No parameters can be null.</exception>
    public PersistedCompilerConfiguration(Assembly coreAssembly, Assembly webAssembly, AssemblyName outputName, string moduleName)
    {
        ArgumentNullException.ThrowIfNull(coreAssembly);
        ArgumentNullException.ThrowIfNull(webAssembly);
        ArgumentNullException.ThrowIfNull(outputName);
        ArgumentNullException.ThrowIfNull(moduleName);

        CoreAssembly = coreAssembly;
        WebAssembly = webAssembly;
        OutputName = outputName;
        ModuleName = moduleName;
    }

    /// <summary>
    /// Gets the reference assembly used to find standard types.
    /// </summary>
    public Assembly CoreAssembly { get; }

    /// <summary>
    /// Gets the assembly used to find the various WebAssembly for .NET types.
    /// </summary>
    public Assembly WebAssembly { get; }

    /// <summary>
    /// Gets the name that will be given to the output.
    /// </summary>
    public AssemblyName OutputName { get; }

    /// <summary>
    /// Gets the name that will be given to the module within the output assembly
    /// </summary>
    public string ModuleName { get; }

    private string typeName = "WebAssembly.CompiledFromWasm";

    /// <summary>
    /// Gets or sets the name of the type that hosts the compiled code.
    /// Defaults to "WebAssembly.CompiledFromWasm".
    /// Intended for use with persisted assembly compilation.
    /// </summary>
    public string TypeName
    {
        get => typeName;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            typeName = value;
        }
    }

    internal sealed override string CompiledTypeName => typeName;

    internal override Type NeutralizeType(Type type)
    {
        switch (type.Namespace)
        {
            case "System":
                if (type.IsGenericType)
                {
                    var genericArguments = type.GetGenericArguments().Select(NeutralizeType).ToArray();
                    var rawType = CoreAssembly.GetType($"{type.Namespace}.{type.Name}")!;
                    return rawType.MakeGenericType(genericArguments);
                }

                return CoreAssembly.GetType($"{type.Namespace}.{type.Name}")!;
            case "WebAssembly.Runtime":
            case "WebAssembly.Runtime.Compilation":
                return WebAssembly.GetType($"{type.Namespace}.{type.Name}")!;
        }

        throw new InvalidOperationException($"Failed to neutralize type {type.Namespace}.{type.Name}.");
    }
}
#endif
