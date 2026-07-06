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
                        if (member is not MethodInfo method)
                            throw new ArgumentException($"Function export '{native.Name}' must be a method, but {member.Name} is a {member.MemberType}.", nameof(exports));

                        var functionDelegate = GetDelegate(method);
                        // Re-register the export delegate with its canonical type ancestry so a re-importing module can
                        // match it by canonical identity (WebAssembly 3.0 GC); host exports carry no descriptor.
                        if (native.Descriptor is { } functionAncestry)
                            WebAssemblyGC.RegisterFunctionReference(functionDelegate, functionAncestry);
                        yield return (native.Name, new FunctionImport(functionDelegate));
                    }
                    continue;

                case ExternalKind.Table:
                    {
                        if (member is not PropertyInfo property)
                            throw new ArgumentException($"Table export '{native.Name}' must be a property, but {member.Name} is a {member.MemberType}.", nameof(exports));

                        var getter = property.GetGetMethod()
                            ?? throw new ArgumentException($"Table export '{native.Name}' requires a public getter on {member.Name}.", nameof(exports));

                        if (getter.Invoke(exports, []) is not TableImport table)
                            throw new ArgumentException($"Table export '{native.Name}' must return a non-null {nameof(TableImport)} from {member.Name}.", nameof(exports));

                        table.Descriptor = native.Descriptor;
                        yield return (native.Name, table);
                    }
                    continue;

                case ExternalKind.Memory:
                    {
                        if (member is not PropertyInfo property)
                            throw new ArgumentException($"Memory export '{native.Name}' must be a property, but {member.Name} is a {member.MemberType}.", nameof(exports));

                        var getter = property.GetGetMethod()
                            ?? throw new ArgumentException($"Memory export '{native.Name}' requires a public getter on {member.Name}.", nameof(exports));

                        yield return (
                            native.Name,
                            new MemoryImport((Func<UnmanagedMemory>)Delegate.CreateDelegate(typeof(Func<UnmanagedMemory>), exports, getter))
                            );
                    }
                    continue;

                case ExternalKind.Global:
                    {
                        if (member is not PropertyInfo property)
                            throw new ArgumentException($"Global export '{native.Name}' must be a property, but {member.Name} is a {member.MemberType}.", nameof(exports));

                        var rawGetter = property.GetGetMethod()
                            ?? throw new ArgumentException($"Global export '{native.Name}' requires a public getter on {member.Name}.", nameof(exports));

                        var rawSetter = property.GetSetMethod();

                        var getter = GetDelegate(rawGetter);
                        var setter = rawSetter != null ? GetDelegate(rawSetter) : null;

                        yield return (native.Name, new GlobalImport(getter, setter) { Descriptor = native.Descriptor });
                    }
                    continue;

                case ExternalKind.Tag:
                    {
                        if (member is not PropertyInfo property)
                            throw new ArgumentException($"Tag export '{native.Name}' must be a property, but {member.Name} is a {member.MemberType}.", nameof(exports));

                        var getter = property.GetGetMethod()
                            ?? throw new ArgumentException($"Tag export '{native.Name}' requires a public getter on {member.Name}.", nameof(exports));

                        if (getter.Invoke(exports, []) is not ExceptionTag tag)
                            throw new ArgumentException($"Tag export '{native.Name}' must return a non-null {nameof(ExceptionTag)} from {member.Name}.", nameof(exports));

                        tag.Descriptor = native.Descriptor;
                        yield return (native.Name, tag);
                    }
                    continue;
            }

            throw new ArgumentException($"Export '{native.Name}' on member {member.Name} has unsupported kind {native.Kind}.", nameof(exports));
        }
    }
}
