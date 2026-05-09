using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Runtime;

/// <summary>
/// Functionality to integrate into a compiled WebAssembly instance.
/// </summary>
public abstract class RuntimeImport
{
    /// <summary>
    /// The type of import.
    /// </summary>
    public abstract ExternalKind Kind { get; }

    private protected RuntimeImport()
    {
    }

    /// <summary>
    /// Uses the <see cref="NativeExportAttribute"/>s on a compiled export to convert it to a sequence of runtime imports.
    /// This can be used to help link one WebAssembly instance to another.
    /// </summary>
    internal static IEnumerable<(string name, RuntimeImport import)> FromCompiledExports<TExports>(TExports exports)
        where TExports : class
    {
        Delegate GetDelegate(MethodInfo method)
        {
            var parameters = method.GetParameters();
            var returns = method.ReturnType;
            if (returns == typeof(void))
                returns = null;
            var del = CompilerConfiguration.GetStandardDelegateForType(parameters.Length, returns != null ? 1 : 0) ??
                throw new ArgumentException($"Unable to produce a compatible delegate for export member {method.Name}.", nameof(exports));
            if (!del.IsGenericTypeDefinition)
                return Delegate.CreateDelegate(del, exports, method);

            var args = method.GetParameters().Select(parm => parm.ParameterType).ToList();
            if (returns != null && returns != typeof(void))
                args.Add(returns);
            return Delegate.CreateDelegate(del.MakeGenericType([.. args]), exports, method);
        }

        foreach (var member in exports.GetType().GetMembers())
        {
            var native = member.GetCustomAttribute<NativeExportAttribute>();
            if (native == null)
                continue;

            switch (native.Kind)
            {
                case ExternalKind.Function:
                    {
                        var method = member as MethodInfo;
                        if (method == null)
                            continue; // TODO: Throw an exception for mismatch.

                        yield return (native.Name, new FunctionImport(GetDelegate(method)));
                    }
                    continue;

                case ExternalKind.Table:
                    {
                        // The attribute is placed on the getter method; handle both getter method and property.
                        MethodInfo? getter = member as MethodInfo ?? (member as PropertyInfo)?.GetGetMethod();
                        if (getter == null)
                            continue;

                        if (getter.Invoke(exports, []) is not TableImport table)
                            continue;

                        yield return (native.Name, table);
                    }
                    continue;

                case ExternalKind.Memory:
                    {
                        MethodInfo? getter = member as MethodInfo ?? (member as PropertyInfo)?.GetGetMethod();
                        if (getter == null)
                            continue;

                        yield return (
                            native.Name,
                            new MemoryImport((Func<UnmanagedMemory>)Delegate.CreateDelegate(typeof(Func<UnmanagedMemory>), exports, getter))
                            );
                    }
                    continue;

                case ExternalKind.Global:
                    {
                        MethodInfo? rawGetter = member as MethodInfo ?? (member as PropertyInfo)?.GetGetMethod();
                        if (rawGetter == null)
                            continue;

                        // For getter-only exports, there's no setter on the method itself; look up the property.
                        var property = member as PropertyInfo ??
                            exports.GetType().GetProperty(rawGetter.Name.StartsWith("get_", StringComparison.Ordinal) ? rawGetter.Name.Substring(4) : rawGetter.Name);
                        var rawSetter = property?.GetSetMethod();

                        var getter = GetDelegate(rawGetter);
                        var setter = rawSetter != null ? GetDelegate(rawSetter) : null;

                        yield return (native.Name, new GlobalImport(getter, setter));
                    }
                    continue;
            }

            // TODO: Throw an exception if no case is hit.
        }
    }
}
