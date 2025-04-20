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
    /// <exception cref="ArgumentNullException"><paramref name="coreAssembly"/> and <paramref name="webAssembly"/> cannot be null.</exception>
    public PersistedCompilerConfiguration(Assembly coreAssembly, Assembly webAssembly)
    {
        ArgumentNullException.ThrowIfNull(coreAssembly);
        ArgumentNullException.ThrowIfNull(webAssembly);

        CoreAssembly = coreAssembly;
        WebAssembly = webAssembly;
    }

    /// <summary>
    /// Gets the reference assembly used to find standard types.
    /// Defaults to the runtime host of the <see cref="Assembly"/> type.
    /// Required when persisted assembly compilation is used to control which version of .NET is required.
    /// </summary>
    public Assembly CoreAssembly { get; }

    /// <summary>
    /// Gets the assembly used to find the various WebAssembly for .NET types.
    /// Defaults to the runtime host of the <see cref="Compile"/> type.
    /// Required when persisted assembly compilation is used to control which version of WebAssembly for .NET is required.
    /// </summary>
    public Assembly WebAssembly { get; }

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
                return WebAssembly.GetType($"{type.Namespace}.{type.Name}")!;
        }

        throw new InvalidOperationException($"Failed to neutralize type {type.Namespace}.{type.Name}.");
    }
}
#endif
