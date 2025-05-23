﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.ExceptionServices;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Runtime;

/// <summary>
/// Creates a new instance of a compiled WebAssembly module.
/// </summary>
/// <typeparam name="TExports">The type of the exports object.</typeparam>
/// <param name="imports">Run-time imports.</param>
/// <returns>The instance.</returns>
/// <exception cref="ArgumentNullException"><paramref name="imports" /> cannot be null.</exception>
public delegate Instance<TExports> InstanceCreator<TExports>(IDictionary<string, IDictionary<string, RuntimeImport>> imports) where TExports : class;

/// <summary>
/// Provides compilation functionality.  Use <see cref="Module"/> for robust inspection and modification capability.
/// </summary>
public static class Compile
{
    /// <summary>
    /// Uses streaming compilation to create an executable <see cref="Instance{TExports}"/> from a binary WebAssembly source.
    /// </summary>
    /// <param name="path">The path to the file that contains a WebAssembly binary stream.</param>
    /// <returns>The module.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="path"/> cannot be null.</exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="path"/> is an empty string (""), contains only white space, or contains one or more invalid characters; or,
    /// <paramref name="path"/> refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in an NTFS environment.
    /// </exception>
    /// <exception cref="NotSupportedException"><paramref name="path"/> refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in a non-NTFS environment.</exception>
    /// <exception cref="FileNotFoundException">The file indicated by <paramref name="path"/> could not be found.</exception>
    /// <exception cref="DirectoryNotFoundException">The specified <paramref name="path"/> is invalid, such as being on an unmapped drive.</exception>
    /// <exception cref="PathTooLongException">
    /// The specified path, file name, or both exceed the system-defined maximum length.
    /// For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters.</exception>
    /// <exception cref="ModuleLoadException">An error was encountered while reading the WebAssembly file.</exception>
    public static InstanceCreator<TExports> FromBinary<TExports>(string path)
    where TExports : class
    {
        return FromBinary<TExports>(path, new CompilerConfiguration());
    }

    /// <summary>
    /// Uses streaming compilation to create an executable <see cref="Instance{TExports}"/> from a binary WebAssembly source.
    /// </summary>
    /// <param name="path">The path to the file that contains a WebAssembly binary stream.</param>
    /// <param name="configuration">Configures the compiler.</param>
    /// <returns>The module.</returns>
    /// <exception cref="ArgumentNullException">No parameters can be null.</exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="path"/> is an empty string (""), contains only white space, or contains one or more invalid characters; or,
    /// <paramref name="path"/> refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in an NTFS environment.
    /// </exception>
    /// <exception cref="NotSupportedException"><paramref name="path"/> refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in a non-NTFS environment.</exception>
    /// <exception cref="FileNotFoundException">The file indicated by <paramref name="path"/> could not be found.</exception>
    /// <exception cref="DirectoryNotFoundException">The specified <paramref name="path"/> is invalid, such as being on an unmapped drive.</exception>
    /// <exception cref="PathTooLongException">
    /// The specified path, file name, or both exceed the system-defined maximum length.
    /// For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters.</exception>
    /// <exception cref="ModuleLoadException">An error was encountered while reading the WebAssembly file.</exception>
    public static InstanceCreator<TExports> FromBinary<TExports>(string path, CompilerConfiguration configuration)
    where TExports : class
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4 * 1024, FileOptions.SequentialScan);
        return FromBinary<TExports>(stream, configuration);
    }

    /// <summary>
    /// Uses streaming compilation to create an executable <see cref="Instance{TExports}"/> from a binary WebAssembly source.
    /// </summary>
    /// <param name="input">The source of data. The stream is left open after reading is complete.</param>
    /// <returns>A function that creates instances on demand.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> cannot be null.</exception>
    public static InstanceCreator<TExports> FromBinary<TExports>(Stream input)
    where TExports : class
    {
        return FromBinary<TExports>(input, new CompilerConfiguration());
    }

    /// <summary>
    /// Uses streaming compilation to create an executable <see cref="Instance{TExports}"/> from a binary WebAssembly source.
    /// </summary>
    /// <param name="input">The source of data. The stream is left open after reading is complete.</param>
    /// <param name="configuration">Configures the compiler.</param>
    /// <returns>A function that creates instances on demand.</returns>
    /// <exception cref="ArgumentNullException">No parameters can be null.</exception>
    public static InstanceCreator<TExports> FromBinary<TExports>(Stream input, CompilerConfiguration configuration)
    where TExports : class
    {
        var exportInfo = typeof(TExports).GetTypeInfo();
        if (!exportInfo.IsPublic && !exportInfo.IsNestedPublic)
            throw new CompilerException($"Export type {exportInfo.FullName} must be public so that the compiler can inherit it.");

        ConstructorInfo constructor;
        using (var reader = new Reader(input))
        {
            try
            {
                CheckPreamble(reader);

                var assembly = AssemblyBuilder.DefineDynamicAssembly(
                    new AssemblyName("CompiledWebAssembly"),
                    AssemblyBuilderAccess.RunAndCollect);

                constructor = FromBinary(
                    assembly,
                    reader,
                    configuration,
                    typeof(Instance<TExports>),
                    typeof(TExports),
                    assembly.DefineDynamicModule("CompiledWebAssembly")
                    );
            }
            catch (OverflowException x)
#if DEBUG
                when (!System.Diagnostics.Debugger.IsAttached)
#endif
            {
                throw new ModuleLoadException("Overflow encountered.", reader.Offset, x);
            }
            catch (EndOfStreamException x)
#if DEBUG
                when (!System.Diagnostics.Debugger.IsAttached)
#endif
            {
                throw new ModuleLoadException("Stream ended unexpectedly.", reader.Offset, x);
            }
            catch (Exception x) when (
            x is not CompilerException
            && x is not ModuleLoadException
#if DEBUG
                && !System.Diagnostics.Debugger.IsAttached
#endif
                )
            {
                throw new ModuleLoadException(x.Message, reader.Offset, x);
            }
        }

        return imports =>
        {
            try
            {
                Func<string, string, RuntimeImport> findImport = (module, field) =>
                {
#if NETSTANDARD
                    if (module == null)
                        throw new ArgumentNullException(nameof(module));
                    if (field == null)
                        throw new ArgumentNullException(nameof(field));
#else
                    ArgumentNullException.ThrowIfNull(module, nameof(module));
                    ArgumentNullException.ThrowIfNull(field, nameof(field));
#endif

                    return !imports.TryGetValue(module, out var fields) || !fields.TryGetValue(field, out var import)
                        ? throw new ImportException($"Missing import for {module}::{field}.")
                        : import;
                };

                return (Instance<TExports>)constructor.Invoke([findImport]);
            }
            catch (TargetInvocationException x)
#if DEBUG
            when (!System.Diagnostics.Debugger.IsAttached)
#endif
            {
                ExceptionDispatchInfo.Capture(x.InnerException ?? x).Throw();
                throw;
            }
        };
    }

    private readonly struct Local(Reader reader)
    {
        public readonly uint Count = reader.ReadVarUInt32();
        public readonly WebAssemblyValueType Type = (WebAssemblyValueType)reader.ReadVarInt7();
    }

    const TypeAttributes ClassAttributes =
        TypeAttributes.Public |
        TypeAttributes.Class |
        TypeAttributes.BeforeFieldInit
        ;

    const MethodAttributes ConstructorAttributes =
        MethodAttributes.Public |
        MethodAttributes.HideBySig |
        MethodAttributes.SpecialName |
        MethodAttributes.RTSpecialName
        ;

    const MethodAttributes InternalFunctionAttributes =
        MethodAttributes.Assembly |
        MethodAttributes.Static |
        MethodAttributes.HideBySig
        ;

    const MethodAttributes ExportedFunctionAttributes =
        MethodAttributes.Public |
        MethodAttributes.Virtual |
        MethodAttributes.Final |
        MethodAttributes.HideBySig
        ;

    const FieldAttributes PrivateReadonlyField =
        FieldAttributes.Private |
        FieldAttributes.InitOnly
        ;

    private static void CheckPreamble(Reader reader)
    {
        if (reader.ReadUInt32() != Module.Magic)
            throw new ModuleLoadException("File preamble magic value is incorrect.", 0);
    }

#if NET9_0_OR_GREATER
    /// <summary>
    /// Creates a <see cref="PersistedAssemblyBuilder"/> from a binary WASM source.
    /// </summary>
    /// <param name="input">The source of data. The stream is left open after reading is complete.</param>
    /// <param name="configuration">Configures the compiler.</param>
    /// <returns>An assembly builder that can be used to further modify or save the assembly.</returns>
    public static PersistedAssemblyBuilder CreatePersistedAssembly(
        Stream input,
        PersistedCompilerConfiguration configuration
        )
    {
        ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

        using var reader = new Reader(input);

        try
        {
            CheckPreamble(reader);

            var assembly = new PersistedAssemblyBuilder(
                configuration.OutputName,
                configuration.CoreAssembly
                );

            var module = assembly.DefineDynamicModule(configuration.ModuleName);

            FromBinary(assembly, reader, configuration, null, null, module, null);

            return assembly;
        }
        catch (OverflowException x)
#if DEBUG
            when (!System.Diagnostics.Debugger.IsAttached)
#endif
        {
            throw new ModuleLoadException("Overflow encountered.", reader.Offset, x);
        }
        catch (EndOfStreamException x)
#if DEBUG
            when (!System.Diagnostics.Debugger.IsAttached)
#endif
        {
            throw new ModuleLoadException("Stream ended unexpectedly.", reader.Offset, x);
        }
        catch (Exception x) when (
        x is not CompilerException
        && x is not ModuleLoadException
#if DEBUG
            && !System.Diagnostics.Debugger.IsAttached
#endif
            )
        {
            throw new ModuleLoadException(x.Message, reader.Offset, x);
        }
    }
#endif

    private static ConstructorInfo FromBinary(
        AssemblyBuilder assembly,
        Reader reader,
        CompilerConfiguration configuration,
        Type? instanceContainer,
        Type? exportContainer,
        ModuleBuilder module,
        TypeBuilder? importBuilder = null
        )
    {
#if NETSTANDARD
        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration));
#else
        ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));
#endif

        switch (reader.ReadUInt32())
        {
            case 0x1: //First release
                break;
            default:
                throw new ModuleLoadException("Unsupported version, only version 0x1 is accepted.", 4);
        }

        uint memoryPagesMinimum;
        uint memoryPagesMaximum = 0;

        Signature[]? signatures = null;
        Signature[]? functionSignatures = null;
        KeyValuePair<string, uint>[]? exportedFunctions = null;
        var previousSection = Section.None;

        var context = new CompilationContext(configuration);
        var exportsBuilder = context.CheckedExportsBuilder = module.DefineType(
            configuration.CompiledTypeName,
            ClassAttributes,
            exportContainer);
        MethodBuilder? importedMemoryProvider = null;
        FieldBuilder? memory = null;

        ILGenerator instanceConstructorIL;
        {
            var instanceConstructor = exportsBuilder.DefineConstructor(
                ConstructorAttributes,
                CallingConventions.Standard,
                [configuration.NeutralizeType(typeof(Func<string, string, RuntimeImport>))]
                );
            instanceConstructor.DefineParameter(1, ParameterAttributes.None, "findImport");
            instanceConstructorIL = instanceConstructor.GetILGenerator();
            {
                if (exportContainer is TypeBuilder buildableExportContainer)
                {
                    var usableConstructor = buildableExportContainer.DefineDefaultConstructor(ConstructorAttributes);
                }
                else
                {
                    var usableConstructor = exportContainer?.GetTypeInfo().DeclaredConstructors.FirstOrDefault(c => c.GetParameters().Length == 0);
                    if (usableConstructor != null)
                    {
                        instanceConstructorIL.Emit(OpCodes.Ldarg_0);
                        instanceConstructorIL.Emit(OpCodes.Call, usableConstructor);
                    }
                }
            }
        }

        var importedFunctions = 0;
        var importedGlobals = 0;
        MethodInfo[]? internalFunctions = null;
        FieldBuilder? functionTable = null;
        GlobalInfo[]? globals = null;
        MethodInfo? startFunction = null;
        var delegateInvokersByTypeIndex = context.DelegateInvokersByTypeIndex;
        var delegateRemappersByType = context.DelegateRemappersByType;
        var emptyTypes = Type.EmptyTypes;

        var preSectionOffset = reader.Offset;
        while (reader.TryReadVarUInt7(out var id)) //At points where TryRead is used, the stream can safely end.
        {
            if (id != 0 && (Section)id < previousSection)
                throw new ModuleLoadException($"Sections out of order; section {(Section)id} encountered after {previousSection}.", preSectionOffset);
            var payloadLength = reader.ReadVarUInt32();

            switch ((Section)id)
            {
                case Section.None:
                    {
                        var preNameOffset = reader.Offset;
                        reader.ReadString(reader.ReadVarUInt32()); //Name
                        reader.ReadBytes(payloadLength - checked((uint)(reader.Offset - preNameOffset))); //Content
                    }
                    break;

                case Section.Type:
                    {
                        signatures = context.Types = new Signature[reader.ReadVarUInt32()];

                        for (var i = 0; i < signatures.Length; i++)
                            signatures[i] = new Signature(reader, (uint)i, configuration);
                    }
                    break;

                case Section.Import:
                    (
                        importedMemoryProvider,
                        importedFunctions,
                        internalFunctions,
                        functionSignatures,
                        importedGlobals,
                        globals,
                        memory,
                        functionTable
                    ) = SectionImport(reader, context, importedMemoryProvider, configuration, signatures, exportsBuilder, instanceConstructorIL, emptyTypes, memory, functionTable);
                    break;

                case Section.Function:
                    {
                        var importedFunctionCount = internalFunctions?.Length ?? 0;
                        var functionIndexSize = checked((int)(importedFunctionCount + reader.ReadVarUInt32()));
                        if (functionSignatures != null)
                        {
                            Array.Resize(ref functionSignatures, functionIndexSize);
                            context.FunctionSignatures = functionSignatures;
                        }
                        else
                        {
                            functionSignatures = context.FunctionSignatures = new Signature[functionIndexSize];
                        }
                        if (importedFunctionCount != 0)
                        {
                            Array.Resize(ref internalFunctions, functionSignatures.Length);
                            context.Methods = internalFunctions;
                        }
                        else
                        {
                            internalFunctions = context.Methods = new MethodInfo[functionSignatures.Length];
                        }

                        if (signatures == null)
                            throw new InvalidOperationException();

                        for (var i = importedFunctionCount; i < functionSignatures.Length; i++)
                        {
                            var signature = functionSignatures[i] = signatures[reader.ReadVarUInt32()];
                            var parms = signature.ParameterTypes.Concat([exportsBuilder]).ToArray();
                            internalFunctions[i] = exportsBuilder.DefineMethod(
                                $"👻 {i}",
                                InternalFunctionAttributes,
                                CallingConventions.Standard,
                                signature.ReturnTypes.FirstOrDefault(),
                                parms
                                );
                        }
                    }
                    break;

                case Section.Table:
                    {
                        var preCountOffset = reader.Offset;
                        var count = reader.ReadVarUInt32();

                        for (var i = 0; i < count; i++)
                        {
                            var preElementTypeOffset = reader.Offset;
                            var elementType = (ElementType)reader.ReadVarInt7();
                            if (elementType != ElementType.FunctionReference)
                                throw new ModuleLoadException($"The only supported table element type is {nameof(ElementType.FunctionReference)}, found {elementType}", preElementTypeOffset);

                            if (functionTable == null)
                            {
                                // It's legal to have multiple tables, but the extra tables are inaccessible to the initial version of WebAssembly.
                                var limits = new ResizableLimits(reader);
                                functionTable = context.FunctionTable = CreateFunctionTableField(exportsBuilder, configuration);
                                instanceConstructorIL.EmitLoadArg(0);
                                instanceConstructorIL.EmitLoadConstant(limits.Minimum);
                                if (limits.Maximum.HasValue)
                                {
                                    instanceConstructorIL.EmitLoadConstant(limits.Maximum);
                                    instanceConstructorIL.Emit(OpCodes.Newobj, configuration.NeutralizeType(typeof(uint?))
                                        .GetTypeInfo()
                                        .DeclaredConstructors
                                        .Where(constructor =>
                                        {
                                            var parms = constructor.GetParameters();
                                            return parms.Length == 1 && parms[0].ParameterType == configuration.NeutralizeType(typeof(uint));
                                        })
                                        .Single());
                                    instanceConstructorIL.Emit(OpCodes.Newobj, configuration.NeutralizeType(typeof(FunctionTable))
                                        .GetTypeInfo()
                                        .DeclaredConstructors
                                        .Where(constructor =>
                                        {
                                            var parms = constructor.GetParameters();
                                            return parms.Length == 2
                                                && parms[0].ParameterType == configuration.NeutralizeType(typeof(uint))
                                                && parms[1].ParameterType == configuration.NeutralizeType(typeof(uint?));
                                        })
                                        .Single());
                                }
                                else
                                {
                                    instanceConstructorIL.Emit(OpCodes.Newobj, configuration.NeutralizeType(typeof(FunctionTable))
                                        .GetTypeInfo()
                                        .DeclaredConstructors
                                        .Where(constructor =>
                                        {
                                            var parms = constructor.GetParameters();
                                            return parms.Length == 1 && parms[0].ParameterType == configuration.NeutralizeType(typeof(uint));
                                        })
                                        .Single());
                                }
                                instanceConstructorIL.Emit(OpCodes.Stfld, functionTable);
                            }
                        }
                    }
                    break;

                case Section.Memory:
                    {
                        var preCountOffset = reader.Offset;
                        var count = reader.ReadVarUInt32();
                        if (count > 1)
                            throw new ModuleLoadException("Multiple memory values are not supported.", preCountOffset);

                        if (count != 0 && memory != null)
                            throw new ModuleLoadException("Memory already provided via import, multiple memory values are not supported.", preCountOffset);

                        var setFlags = (ResizableLimits.Flags)reader.ReadVarUInt32();
                        memoryPagesMinimum = reader.ReadVarUInt32();
                        if ((setFlags & ResizableLimits.Flags.Maximum) != 0)
                            memoryPagesMaximum = Math.Min(reader.ReadVarUInt32(), uint.MaxValue / Memory.PageSize);
                        else
                            memoryPagesMaximum = uint.MaxValue / Memory.PageSize;

                        memory = context.Memory = CreateMemoryField(exportsBuilder, configuration);

                        instanceConstructorIL.Emit(OpCodes.Ldarg_0);
                        Instructions.Int32Constant.Emit(instanceConstructorIL, (int)memoryPagesMinimum);
                        Instructions.Int32Constant.Emit(instanceConstructorIL, (int)memoryPagesMaximum);
                        instanceConstructorIL.Emit(OpCodes.Newobj, configuration.NeutralizeType(typeof(uint?)).GetTypeInfo().DeclaredConstructors.Where(info =>
                        {
                            var parms = info.GetParameters();
                            return parms.Length == 1 && parms[0].ParameterType == configuration.NeutralizeType(typeof(uint));
                        }).First());

                        instanceConstructorIL.Emit(OpCodes.Newobj, configuration.NeutralizeType(typeof(UnmanagedMemory)).GetTypeInfo().DeclaredConstructors.Where(info =>
                        {
                            var parms = info.GetParameters();
                            return
                                parms.Length == 2 &&
                                parms[0].ParameterType == configuration.NeutralizeType(typeof(uint)) &&
                                parms[1].ParameterType == configuration.NeutralizeType(typeof(uint?));
                        }).First());

                        instanceConstructorIL.Emit(OpCodes.Stfld, memory);

                        exportsBuilder.AddInterfaceImplementation(configuration.NeutralizeType(typeof(IDisposable)));

                        var dispose = exportsBuilder.DefineMethod(
                            "Dispose",
                            MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot,
                            CallingConventions.HasThis,
                            configuration.NeutralizeType(typeof(void)),
                            emptyTypes
                            );

                        var disposeIL = dispose.GetILGenerator();
                        disposeIL.Emit(OpCodes.Ldarg_0);
                        disposeIL.Emit(OpCodes.Ldfld, memory);
                        disposeIL.Emit(OpCodes.Call, configuration.NeutralizeType(typeof(UnmanagedMemory))
                            .GetTypeInfo()
                            .DeclaredMethods
                            .Where(info =>
                            info.ReturnType == configuration.NeutralizeType(typeof(void))
                            && info.GetParameters().Length == 0
                            && info.Name == nameof(UnmanagedMemory.Dispose))
                            .First());
                        disposeIL.Emit(OpCodes.Ret);
                    }
                    break;

                case Section.Global:
                    globals = SectionGlobal(reader, context, globals, exportsBuilder, instanceConstructorIL, importedGlobals, configuration);
                    break;

                case Section.Export:
                    exportedFunctions = SectionExport(reader, functionTable, exportsBuilder, emptyTypes, memory, globals, configuration);
                    break;

                case Section.Start:
                    if (internalFunctions == null)
                        throw new ModuleLoadException("Start section created without any functions.", preSectionOffset);
                    startFunction = SectionStart(reader, internalFunctions);
                    break;

                case Section.Element:
                    if (functionTable == null)
                        throw new ModuleLoadException("Element section found without an associated table section or import.", preSectionOffset);
                    SectionElement(reader, functionTable, instanceConstructorIL, functionSignatures, internalFunctions, delegateInvokersByTypeIndex, configuration, exportsBuilder);
                    break;

                case Section.Code:
                    if (functionSignatures == null)
                        throw new InvalidOperationException($"Code section found but {nameof(functionSignatures)} is null");
                    if (internalFunctions == null)
                        throw new InvalidOperationException($"Code section found but {nameof(internalFunctions)} is null");
                    SectionCode(reader, context, functionSignatures, internalFunctions, importedFunctions);
                    break;

                case Section.Data:
                    if (memory == null)
                        throw new ModuleLoadException("Data section cannot be used unless a memory section is defined.", preSectionOffset);
                    SectionData(reader, context, memory, instanceConstructorIL, exportsBuilder);
                    break;

                default:
                    throw new ModuleLoadException($"Unrecognized section type {(Section)id}.", preSectionOffset);
            }

            preSectionOffset = reader.Offset;
            previousSection = (Section)id;
        }

        if (exportedFunctions != null && exportedFunctions.Length != 0)
        {
            if (functionSignatures == null)
                throw new InvalidOperationException();
            if (internalFunctions == null)
                throw new InvalidOperationException();

            for (var i = 0; i < exportedFunctions.Length; i++)
            {
                var exported = exportedFunctions[i];
                var signature = functionSignatures[exported.Value];

                var method = exportsBuilder.DefineMethod(
                    NameCleaner.CleanName(exported.Key),
                    ExportedFunctionAttributes,
                    CallingConventions.HasThis,
                    signature.ReturnTypes.FirstOrDefault(),
                    signature.ParameterTypes
                    );
#if NET9_0_OR_GREATER
                if (configuration is not PersistedCompilerConfiguration) // Need to redesign this for persisted.
#endif
                method.SetCustomAttribute(NativeExportAttribute.Emit(ExternalKind.Function, exported.Key));

                var il = method.GetILGenerator();
                for (var parm = 0; parm < signature.ParameterTypes.Length; parm++)
                    il.Emit(OpCodes.Ldarg, parm + 1);

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Call, internalFunctions[exported.Value]);
                il.Emit(OpCodes.Ret);
            }
        }

        if (startFunction != null)
        {
            instanceConstructorIL.Emit(OpCodes.Ldarg_0);
            instanceConstructorIL.Emit(OpCodes.Call, startFunction);
        }

        instanceConstructorIL.Emit(OpCodes.Ret); //Finish the constructor.
        var exportInfo = exportsBuilder.CreateTypeInfo();

        if (importBuilder is null && instanceContainer is null)
            return exportInfo.DeclaredConstructors.First();

        TypeInfo instance;
        {
            var instanceBuilder = module.DefineType("CompiledInstance", ClassAttributes, instanceContainer);
            var instanceConstructor = instanceBuilder.DefineConstructor(
                ConstructorAttributes,
                CallingConventions.Standard,
                [typeof(Func<string, string, RuntimeImport>)]
                );
            var il = instanceConstructor.GetILGenerator();
            var memoryAllocated = checked(memoryPagesMaximum * Memory.PageSize);

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Newobj, exportInfo.DeclaredConstructors.First());

            ConstructorInfo importConstructor;
            if (importBuilder is not null)
            {
                var importConstructorBuilder = importBuilder.DefineConstructor(ConstructorAttributes, CallingConventions.Standard, []);

                var importConstructorIL = importConstructorBuilder.GetILGenerator();
                importConstructorIL.Emit(OpCodes.Ldarg_0);
                importConstructorIL.Emit(OpCodes.Call, typeof(object).GetConstructor([])!);
                importConstructorIL.Emit(OpCodes.Ret);

                importConstructor = importConstructorBuilder;
            }
            else if (instanceContainer is not null)
            {
                importConstructor = instanceContainer
                    .GetTypeInfo()
                    .DeclaredConstructors
                    .First(info => info.GetParameters()
                    .FirstOrDefault()
                    ?.ParameterType == exportContainer
                    );
            }
            else
            {
                return exportInfo.DeclaredConstructors.First();
            }

            il.Emit(OpCodes.Call, importConstructor);
            il.Emit(OpCodes.Ret);

            instance = instanceBuilder.CreateTypeInfo();
        }

        module.CreateGlobalFunctions();
        return instance.DeclaredConstructors.First();
    }

    private static (
        MethodBuilder? importedMemoryProvider,
        int importedFunctions,
        MethodInfo[] internalFunctions,
        Signature[] functionSignatures,
        int importedGlobals,
        GlobalInfo[] globals,
        FieldBuilder? memory,
        FieldBuilder? functionTable
    ) SectionImport(Reader reader, CompilationContext context, MethodBuilder? importedMemoryProvider, CompilerConfiguration configuration, Signature[]? signatures, TypeBuilder exportsBuilder, ILGenerator instanceConstructorIL, Type[] emptyTypes, FieldBuilder? memory, FieldBuilder? functionTable)
    {
        var count = checked((int)reader.ReadVarUInt32());
        var functionImports = new List<MethodInfo>(count);
        var functionImportTypes = new List<Signature>(count);
        var globalImports = new List<GlobalInfo>(count);
        var missingDelegates = new List<MissingDelegateType>();
        var importFinderInvoke = configuration.NeutralizeType(typeof(Func<string, string, RuntimeImport>)).GetMethod("Invoke")!;

        for (var i = 0; i < count; i++)
        {
            var moduleName = reader.ReadString(reader.ReadVarUInt32());
            var fieldName = reader.ReadString(reader.ReadVarUInt32());

            var preKindOffset = reader.Offset;
            var kind = (ExternalKind)reader.ReadByte();

            switch (kind)
            {
                case ExternalKind.Function:
                    {
                        var preTypeIndexOffset = reader.Offset;
                        var typeIndex = reader.ReadVarUInt32();
                        if (signatures == null)
                            throw new InvalidOperationException();
                        if (typeIndex >= signatures.Length)
                            throw new ModuleLoadException($"Requested type index {typeIndex} but only {signatures.Length} are available.", preTypeIndexOffset);
                        var signature = signatures[typeIndex];
                        var del = configuration.GetDelegateForType(signature.ParameterTypes.Length, signature.ReturnTypes.Length);
                        if (del == null)
                        {
                            missingDelegates.Add(new MissingDelegateType(moduleName, fieldName, signature));
                            continue;
                        }

                        var typedDelegate = configuration.NeutralizeType(del.IsGenericTypeDefinition ? del.MakeGenericType([.. signature.ParameterTypes, .. signature.ReturnTypes]) : del);
                        var delField = $"➡ {moduleName}::{fieldName}";
                        var delFieldBuilder = exportsBuilder.DefineField(delField, typedDelegate, PrivateReadonlyField);

                        var invoker = exportsBuilder.DefineMethod(
                            $"Invoke {delField}",
                            InternalFunctionAttributes,
                            CallingConventions.Standard,
                            signature.ReturnTypes.Length != 0 ? signature.ReturnTypes[0] : null,
                            [.. signature.ParameterTypes, exportsBuilder]
                            );

                        var invokerIL = invoker.GetILGenerator();
                        invokerIL.EmitLoadArg(signature.ParameterTypes.Length);
                        invokerIL.Emit(OpCodes.Ldfld, delFieldBuilder);

                        for (ushort arg = 0; arg < signature.ParameterTypes.Length; arg++)
                        {
                            invokerIL.EmitLoadArg(arg);
                        }

                        invokerIL.Emit(OpCodes.Callvirt, typedDelegate.GetRuntimeMethod(nameof(Action.Invoke), signature.ParameterTypes)!);
                        invokerIL.Emit(OpCodes.Ret);

                        instanceConstructorIL.Emit(OpCodes.Ldarg_0);
                        instanceConstructorIL.Emit(OpCodes.Ldarg_1);
                        instanceConstructorIL.Emit(OpCodes.Ldstr, moduleName);
                        instanceConstructorIL.Emit(OpCodes.Ldstr, fieldName);
                        instanceConstructorIL.Emit(OpCodes.Callvirt, importFinderInvoke);

                        ImportException.EmitTryCast(instanceConstructorIL, configuration.NeutralizeType(typeof(FunctionImport)), configuration);

                        instanceConstructorIL.Emit(OpCodes.Callvirt,
                            configuration.NeutralizeType(typeof(FunctionImport))
                            .GetTypeInfo()
                            .GetDeclaredProperty(nameof(FunctionImport.Method))!
                            .GetMethod!);
                        ImportException.EmitTryCast(instanceConstructorIL, typedDelegate, configuration);
                        instanceConstructorIL.Emit(OpCodes.Stfld, delFieldBuilder);

                        functionImports.Add(invoker);
                        functionImportTypes.Add(signature);
                    }
                    break;
                case ExternalKind.Table:
                    {
                        var preElementTypeoffset = reader.Offset;
                        var elementType = (ElementType)reader.ReadVarInt7();
                        if (elementType != ElementType.FunctionReference)
                            throw new ModuleLoadException($"{moduleName}::{fieldName} imported table type of  kind of {elementType} is not recognized.", preElementTypeoffset);

                        var limits = new ResizableLimits(reader);

                        if (functionTable != null)
                            throw new NotSupportedException("Unable to support multiple tables.");

                        functionTable = context.FunctionTable = CreateFunctionTableField(exportsBuilder, configuration);
                        instanceConstructorIL.Emit(OpCodes.Ldarg_0);
                        instanceConstructorIL.Emit(OpCodes.Ldarg_1);
                        instanceConstructorIL.Emit(OpCodes.Ldstr, moduleName);
                        instanceConstructorIL.Emit(OpCodes.Ldstr, fieldName);
                        instanceConstructorIL.Emit(OpCodes.Callvirt, importFinderInvoke);

                        ImportException.EmitTryCast(instanceConstructorIL, configuration.NeutralizeType(typeof(FunctionTable)), configuration);

                        instanceConstructorIL.Emit(OpCodes.Stfld, functionTable);
                    }
                    break;
                case ExternalKind.Memory:
                    {
                        var limits = new ResizableLimits(reader);

                        var typedDelegate = typeof(Func<UnmanagedMemory>);
                        var delField = $"➡ {moduleName}::{fieldName}";
                        var delFieldBuilder = exportsBuilder.DefineField(delField, typedDelegate, PrivateReadonlyField);

                        importedMemoryProvider = exportsBuilder.DefineMethod(
                            $"Invoke {delField}",
                            InternalFunctionAttributes,
                            CallingConventions.Standard,
                            typeof(UnmanagedMemory),
                            [exportsBuilder]
                            );

                        var invokerIL = importedMemoryProvider.GetILGenerator();
                        invokerIL.EmitLoadArg(0);
                        invokerIL.Emit(OpCodes.Ldfld, delFieldBuilder);
                        invokerIL.Emit(OpCodes.Callvirt, typedDelegate.GetRuntimeMethod(nameof(Func<UnmanagedMemory>.Invoke), emptyTypes)!);
                        invokerIL.Emit(OpCodes.Ret);

                        instanceConstructorIL.Emit(OpCodes.Ldarg_0);
                        instanceConstructorIL.Emit(OpCodes.Ldarg_1);
                        instanceConstructorIL.Emit(OpCodes.Ldstr, moduleName);
                        instanceConstructorIL.Emit(OpCodes.Ldstr, fieldName);
                        instanceConstructorIL.Emit(OpCodes.Callvirt, importFinderInvoke);

                        ImportException.EmitTryCast(instanceConstructorIL, typeof(MemoryImport), configuration);

                        instanceConstructorIL.Emit(OpCodes.Callvirt,
                            typeof(MemoryImport)
                            .GetTypeInfo()
                            .GetDeclaredProperty(nameof(MemoryImport.Method))!
                            .GetMethod!);
                        ImportException.EmitTryCast(instanceConstructorIL, typedDelegate, configuration);
                        instanceConstructorIL.Emit(OpCodes.Stfld, delFieldBuilder);

                        memory = context.Memory = CreateMemoryField(exportsBuilder, configuration);
                        instanceConstructorIL.Emit(OpCodes.Ldarg_0);
                        instanceConstructorIL.Emit(OpCodes.Ldarg_0);
                        instanceConstructorIL.Emit(OpCodes.Call, importedMemoryProvider);
                        instanceConstructorIL.Emit(OpCodes.Stfld, memory);
                    }
                    break;
                case ExternalKind.Global:
                    {
                        var contentType = (WebAssemblyValueType)reader.ReadVarInt7();
                        var mutable = reader.ReadVarUInt1() == 1;

                        var typedDelegate = configuration.NeutralizeType(typeof(Func<>).MakeGenericType([contentType.ToSystemType()]));
                        var delField = $"➡ Get {moduleName}::{fieldName}";
                        var delFieldBuilder = exportsBuilder.DefineField(delField, typedDelegate, PrivateReadonlyField);

                        var getterInvoker = exportsBuilder.DefineMethod(
                            $"Invoke {delField}",
                            InternalFunctionAttributes,
                            CallingConventions.Standard,
                            configuration.NeutralizeType(contentType.ToSystemType()),
                            [exportsBuilder]
                            );

                        var invokerIL = getterInvoker.GetILGenerator();
                        invokerIL.EmitLoadArg(0);
                        invokerIL.Emit(OpCodes.Ldfld, delFieldBuilder);
                        invokerIL.Emit(OpCodes.Callvirt, typedDelegate.GetRuntimeMethod(nameof(Func<WebAssemblyValueType>.Invoke), emptyTypes)!);
                        invokerIL.Emit(OpCodes.Ret);

                        instanceConstructorIL.Emit(OpCodes.Ldarg_0);
                        instanceConstructorIL.Emit(OpCodes.Ldarg_1);
                        instanceConstructorIL.Emit(OpCodes.Ldstr, moduleName);
                        instanceConstructorIL.Emit(OpCodes.Ldstr, fieldName);
                        instanceConstructorIL.Emit(OpCodes.Callvirt, importFinderInvoke);

                        ImportException.EmitTryCast(instanceConstructorIL, typeof(GlobalImport), configuration);

                        instanceConstructorIL.Emit(OpCodes.Callvirt,
                             typeof(GlobalImport)
                            .GetTypeInfo()
                            .GetDeclaredProperty(nameof(GlobalImport.Getter))!
                            .GetMethod!);
                        ImportException.EmitTryCast(instanceConstructorIL, typedDelegate, configuration);
                        instanceConstructorIL.Emit(OpCodes.Stfld, delFieldBuilder);

                        MethodBuilder? setterInvoker;
                        if (!mutable)
                        {
                            setterInvoker = null;
                        }
                        else
                        {
                            typedDelegate = configuration.NeutralizeType(typeof(Action<>).MakeGenericType([contentType.ToSystemType()]));
                            delField = $"➡ Set {moduleName}::{fieldName}";
                            delFieldBuilder = exportsBuilder.DefineField(delField, typedDelegate, PrivateReadonlyField);

                            setterInvoker = exportsBuilder.DefineMethod(
                            $"Invoke {delField}",
                            InternalFunctionAttributes,
                            CallingConventions.Standard,
                            null,
                            [configuration.NeutralizeType(contentType.ToSystemType()), exportsBuilder]
                            );

                            invokerIL = setterInvoker.GetILGenerator();
                            invokerIL.EmitLoadArg(1);
                            invokerIL.Emit(OpCodes.Ldfld, delFieldBuilder);
                            invokerIL.EmitLoadArg(0);
                            invokerIL.Emit(OpCodes.Callvirt, typedDelegate.GetRuntimeMethod(nameof(Action<WebAssemblyValueType>.Invoke), [configuration.NeutralizeType(contentType.ToSystemType())])!);
                            invokerIL.Emit(OpCodes.Ret);

                            instanceConstructorIL.Emit(OpCodes.Ldarg_0);
                            instanceConstructorIL.Emit(OpCodes.Ldarg_1);
                            instanceConstructorIL.Emit(OpCodes.Ldstr, moduleName);
                            instanceConstructorIL.Emit(OpCodes.Ldstr, fieldName);
                            instanceConstructorIL.Emit(OpCodes.Callvirt, importFinderInvoke);

                            ImportException.EmitTryCast(instanceConstructorIL, typeof(GlobalImport), configuration);

                            instanceConstructorIL.Emit(OpCodes.Callvirt,
                                typeof(GlobalImport)
                                .GetTypeInfo()
                                .GetDeclaredProperty(nameof(GlobalImport.Setter))!
                                .GetMethod!);
                            ImportException.EmitTryCast(instanceConstructorIL, typedDelegate, configuration);
                            instanceConstructorIL.Emit(OpCodes.Stfld, delFieldBuilder);
                        }

                        globalImports.Add(new GlobalInfo(contentType, true, getterInvoker, setterInvoker));
                    }
                    break;
                default:
                    throw new ModuleLoadException($"{moduleName}::{fieldName} imported external kind of {kind} is not recognized.", preKindOffset);
            }
        }

        if (missingDelegates.Count != 0)
            throw new MissingDelegateTypesException(missingDelegates);

        context.Methods = [.. functionImports];
        context.FunctionSignatures = [.. functionImportTypes];
        context.Globals = [.. globalImports];

        return (
            importedMemoryProvider,
            functionImports.Count,
            context.Methods,
            context.FunctionSignatures,
            globalImports.Count,
            context.Globals,
            memory,
            functionTable
        );
    }

    static GlobalInfo[] SectionGlobal(
        Reader reader,
        CompilationContext context,
        GlobalInfo[]? globals,
        TypeBuilder exportsBuilder,
        ILGenerator instanceConstructorIL,
        int importedGlobals,
        CompilerConfiguration configuration)
    {
        var count = reader.ReadVarUInt32();
        if (globals != null)
        {
            Array.Resize(ref globals, checked((int)(globals.Length + count)));
            context.Globals = globals;
        }
        else
        {
            globals = context.Globals = new GlobalInfo[count];
        }

        var emptySignature = Signature.Empty;

        for (var i = 0; i < count; i++)
        {
            var contentType = (WebAssemblyValueType)reader.ReadVarInt7();
            var isMutable = reader.ReadVarUInt1() == 1;

            var getter = exportsBuilder.DefineMethod(
                $"🌍 Get {i}",
                InternalFunctionAttributes,
                CallingConventions.Standard,
                configuration.NeutralizeType(contentType.ToSystemType()),
                isMutable ? [exportsBuilder] : null
                );

            var il = getter.GetILGenerator();
            var getterSignature = new Signature(contentType, configuration);
            MethodBuilder? setter;

            if (!isMutable)
            {
                context.Reset(
                    il,
                    getterSignature,
                    getterSignature.RawParameterTypes
                    );

                foreach (var instruction in Instruction.ParseInitializerExpression(reader))
                {
                    instruction.Compile(context);
                    context.Previous = instruction.OpCode;
                }

                setter = null;
            }
            else //Mutable
            {
                var field = exportsBuilder.DefineField(
                    $"🌍 {i}",
                    configuration.NeutralizeType(contentType.ToSystemType()),
                    FieldAttributes.Private | (isMutable ? 0 : FieldAttributes.InitOnly)
                    );

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, field);
                il.Emit(OpCodes.Ret);

                setter = exportsBuilder.DefineMethod(
                $"🌍 Set {i}",
                    InternalFunctionAttributes,
                    CallingConventions.Standard,
                    configuration.NeutralizeType(typeof(void)),
                    [configuration.NeutralizeType(contentType.ToSystemType()), exportsBuilder]
                    );

                il = setter.GetILGenerator();
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Stfld, field);
                il.Emit(OpCodes.Ret);

                context.Reset(
                    instanceConstructorIL,
                    emptySignature,
                    emptySignature.RawParameterTypes
                    );

                context.EmitLoadThis();
                var ended = false;

                foreach (var instruction in Instruction.ParseInitializerExpression(reader))
                {
                    if (ended)
                        throw new CompilerException("Only a single End is allowed within an initializer expression.");

                    if (instruction.OpCode == OpCode.End)
                    {
                        context.Emit(OpCodes.Stfld, field);
                        ended = true;
                        continue;
                    }

                    instruction.Compile(context);
                    context.Previous = instruction.OpCode;
                }
            }

            globals[importedGlobals + i] = new GlobalInfo(contentType, isMutable, getter, setter);
        }

        return globals;
    }

    static KeyValuePair<string, uint>[] SectionExport(
        Reader reader,
        FieldBuilder? functionTable,
        TypeBuilder exportsBuilder,
        Type[] emptyTypes,
        FieldBuilder? memory,
        GlobalInfo[]? globals,
        CompilerConfiguration configuration)
    {
        const MethodAttributes exportedPropertyAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.Virtual | MethodAttributes.Final;
        var totalExports = reader.ReadVarUInt32();
        var xFunctions = new List<KeyValuePair<string, uint>>((int)Math.Min(int.MaxValue, totalExports));

        for (var i = 0; i < totalExports; i++)
        {
            var name = reader.ReadString(reader.ReadVarUInt32());
            var preKindOffset = reader.Offset;
            var kind = (ExternalKind)reader.ReadByte();
            var preIndexOffset = reader.Offset;
            var index = reader.ReadVarUInt32();
            switch (kind)
            {
                case ExternalKind.Function:
                    xFunctions.Add(new KeyValuePair<string, uint>(name, index));
                    break;
                case ExternalKind.Table:
                    if (index != 0)
                        throw new ModuleLoadException($"Exported table must be of index 0, found {index}.", preIndexOffset);
                    if (functionTable == null)
                        throw new ModuleLoadException("Can't export a table without defining or importing one.", preKindOffset);

                    {
                        var tableGetter = exportsBuilder.DefineMethod("get_" + name,
                            exportedPropertyAttributes,
                            CallingConventions.HasThis,
                            configuration.NeutralizeType(typeof(FunctionTable)),
                            emptyTypes
                            );
#if NET9_0_OR_GREATER
                        if (configuration is not PersistedCompilerConfiguration) // Need to redesign this for persisted.
#endif
                        tableGetter.SetCustomAttribute(NativeExportAttribute.Emit(ExternalKind.Table, name));
                        var getterIL = tableGetter.GetILGenerator();
                        getterIL.Emit(OpCodes.Ldarg_0);
                        getterIL.Emit(OpCodes.Ldfld, functionTable);
                        getterIL.Emit(OpCodes.Ret);

                        exportsBuilder.DefineProperty(name, PropertyAttributes.None, configuration.NeutralizeType(typeof(FunctionTable)), emptyTypes)
                            .SetGetMethod(tableGetter);
                    }
                    break;
                case ExternalKind.Memory:
                    if (index != 0)
                        throw new ModuleLoadException($"Exported memory must be of index 0, found {index}.", preIndexOffset);
                    if (memory == null)
                        throw new CompilerException("Cannot export linear memory when linear memory is not defined.");

                    {
                        var memoryGetter = exportsBuilder.DefineMethod("get_" + name,
                            exportedPropertyAttributes,
                            CallingConventions.HasThis,
                            configuration.NeutralizeType(typeof(UnmanagedMemory)),
                            emptyTypes
                            );
#if NET9_0_OR_GREATER
                        if (configuration is not PersistedCompilerConfiguration) // Need to redesign this for persisted.
#endif
                        memoryGetter.SetCustomAttribute(NativeExportAttribute.Emit(ExternalKind.Memory, name));
                        var getterIL = memoryGetter.GetILGenerator();
                        getterIL.Emit(OpCodes.Ldarg_0);
                        getterIL.Emit(OpCodes.Ldfld, memory);
                        getterIL.Emit(OpCodes.Ret);

                        exportsBuilder.DefineProperty(name, PropertyAttributes.None, configuration.NeutralizeType(typeof(UnmanagedMemory)), emptyTypes)
                            .SetGetMethod(memoryGetter);
                    }
                    break;
                case ExternalKind.Global:
                    if (globals == null)
                        throw new ModuleLoadException($"Exported index {index} is global but no globals are defined.", preIndexOffset);
                    if (index >= globals.Length)
                        throw new ModuleLoadException($"Exported global index of {index} is greater than the number of globals {globals.Length}.", preIndexOffset);

                    {
                        var global = globals[index];
                        var property = exportsBuilder.DefineProperty(name, PropertyAttributes.None, configuration.NeutralizeType(global.Type.ToSystemType()), emptyTypes);
#if NET9_0_OR_GREATER
                        if (configuration is not PersistedCompilerConfiguration) // Need to redesign this for persisted.
#endif
                        property.SetCustomAttribute(NativeExportAttribute.Emit(ExternalKind.Global, name));
                        var wrappedGet = exportsBuilder.DefineMethod("get_" + name,
                            exportedPropertyAttributes,
                            CallingConventions.HasThis,
                            configuration.NeutralizeType(global.Type.ToSystemType()),
                            emptyTypes
                            );

                        var wrappedGetIL = wrappedGet.GetILGenerator();
                        if (global.RequiresInstance)
                            wrappedGetIL.Emit(OpCodes.Ldarg_0);
                        wrappedGetIL.Emit(OpCodes.Call, global.Getter);
                        wrappedGetIL.Emit(OpCodes.Ret);
                        property.SetGetMethod(wrappedGet);

                        var setter = global.Setter;
                        if (setter != null)
                        {
                            var wrappedSet = exportsBuilder.DefineMethod("set_" + name,
                                exportedPropertyAttributes,
                                CallingConventions.HasThis,
                                null,
                                [configuration.NeutralizeType(global.Type.ToSystemType())]
                                );

                            var wrappedSetIL = wrappedSet.GetILGenerator();
                            wrappedSetIL.Emit(OpCodes.Ldarg_1);
                            if (global.RequiresInstance)
                                wrappedSetIL.Emit(OpCodes.Ldarg_0);
                            wrappedSetIL.Emit(OpCodes.Call, setter);
                            wrappedSetIL.Emit(OpCodes.Ret);

                            property.SetSetMethod(wrappedSet);
                        }
                    }
                    break;
                default:
                    throw new NotSupportedException($"Unrecognized export kind {kind}.");
            }
        }

        return [.. xFunctions];
    }

    static MethodInfo SectionStart(Reader reader, MethodInfo[] internalFunctions)
    {
        var preReadOffset = reader.Offset;
        var startIndex = reader.ReadVarInt32();
        if (startIndex >= internalFunctions.Length)
            throw new ModuleLoadException($"Start function of index {startIndex} exceeds available functions of {internalFunctions.Length}", preReadOffset);

        return internalFunctions[startIndex];
    }

    static void SectionElement(Reader reader, FieldBuilder functionTable, ILGenerator instanceConstructorIL, Signature[]? functionSignatures, MethodInfo[]? internalFunctions, Dictionary<uint, MethodInfo> delegateInvokersByTypeIndex, CompilerConfiguration configuration, TypeBuilder exportsBuilder)
    {
        // Holds the function table index of where an exsting function index has been mapped, for re-use.
        var existingDelegates = new Dictionary<uint, uint>();

        var count = reader.ReadVarUInt32();

        if (count == 0)
            return;

        var localFunctionTable = instanceConstructorIL.DeclareLocal(typeof(FunctionTable));
        instanceConstructorIL.EmitLoadArg(0);
        instanceConstructorIL.Emit(OpCodes.Ldfld, functionTable);
        instanceConstructorIL.Emit(OpCodes.Stloc, localFunctionTable);

        var getter = FunctionTable.IndexGetter;
        var setter = FunctionTable.IndexSetter;

        for (var i = 0; i < count; i++)
        {
            var preIndexOffset = reader.Offset;
            var index = reader.ReadVarUInt32();
            if (index != 0)
                throw new ModuleLoadException($"Index value of anything other than 0 is not supported, {index} found.", preIndexOffset);

            uint offset;
            {
                var preInitializerOffset = reader.Offset;
                var initializer = Instruction.ParseInitializerExpression(reader).ToArray();
                if (initializer.Length != 2 || initializer[0] is not Instructions.Int32Constant c || initializer[1] is not Instructions.End)
                    throw new ModuleLoadException("Initializer expression support for the Element section is limited to a single Int32 constant followed by end.", preInitializerOffset);

                offset = (uint)c.Value;
            }

            var preElementOffset = reader.Offset;
            var elements = reader.ReadVarUInt32();

            if (elements == 0)
                continue;

            if (functionSignatures == null || internalFunctions == null)
                throw new ModuleLoadException("Element section must be empty if there are no functions to reference.", preElementOffset);

            var isBigEnough = instanceConstructorIL.DefineLabel();
            instanceConstructorIL.Emit(OpCodes.Ldloc, localFunctionTable);
            instanceConstructorIL.Emit(OpCodes.Call, FunctionTable.LengthGetter);
            instanceConstructorIL.EmitLoadConstant(checked(offset + elements));
            instanceConstructorIL.Emit(OpCodes.Bge_Un, isBigEnough);

            instanceConstructorIL.Emit(OpCodes.Ldloc, localFunctionTable);
            instanceConstructorIL.EmitLoadConstant(checked(offset + elements));
            instanceConstructorIL.Emit(OpCodes.Ldloc, localFunctionTable);
            instanceConstructorIL.Emit(OpCodes.Call, FunctionTable.LengthGetter);
            instanceConstructorIL.Emit(OpCodes.Sub);
            instanceConstructorIL.Emit(OpCodes.Call, FunctionTable.GrowMethod);
            instanceConstructorIL.Emit(OpCodes.Pop);

            instanceConstructorIL.MarkLabel(isBigEnough);

            for (var j = 0u; j < elements; j++)
            {
                var functionIndex = reader.ReadVarUInt32();
                var signature = functionSignatures[functionIndex];
                var parms = signature.ParameterTypes;
                var returns = signature.ReturnTypes;

                if (!delegateInvokersByTypeIndex.TryGetValue(signature.TypeIndex, out var invoker))
                {
                    var del = configuration.GetDelegateForType(parms.Length, returns.Length) ??
                        throw new CompilerException($"Failed to get a delegate for type {signature}.");
                    if (del.IsGenericType)
                        del = del.MakeGenericType([.. parms, .. returns]);

                    delegateInvokersByTypeIndex.Add(signature.TypeIndex, invoker = del.GetTypeInfo().GetDeclaredMethod(nameof(Action.Invoke))!);
                }

                instanceConstructorIL.Emit(OpCodes.Ldloc, localFunctionTable);
                instanceConstructorIL.EmitLoadConstant(offset + j);

                if (existingDelegates.TryGetValue(functionIndex, out var existing))
                {
                    instanceConstructorIL.Emit(OpCodes.Ldloc, localFunctionTable);
                    instanceConstructorIL.EmitLoadConstant(existing);
                    instanceConstructorIL.Emit(OpCodes.Call, getter);
                }
                else
                {
                    existingDelegates.Add(functionIndex, offset + j);

                    var wrapper = exportsBuilder.DefineMethod(
                        $"📦 {functionIndex}",
                        MethodAttributes.Private | MethodAttributes.HideBySig,
                        returns.Length == 0 ? typeof(void) : returns[0],
                        parms
                        );

                    var il = wrapper.GetILGenerator();
                    for (var k = 0; k < parms.Length; k++)
                        il.EmitLoadArg(k + 1);
                    il.EmitLoadArg(0);
                    il.Emit(OpCodes.Call, internalFunctions[functionIndex]);
                    il.Emit(OpCodes.Ret);

                    instanceConstructorIL.EmitLoadArg(0);
                    instanceConstructorIL.Emit(OpCodes.Ldftn, wrapper);
                    instanceConstructorIL.Emit(OpCodes.Newobj, invoker.DeclaringType!.GetTypeInfo().DeclaredConstructors.Single());
                }

                instanceConstructorIL.Emit(OpCodes.Call, setter);
            }
        }
    }

    static void SectionCode(Reader reader, CompilationContext context, Signature[] functionSignatures, MethodInfo[] internalFunctions, int importedFunctions)
    {
        var preBodiesIndex = reader.Offset;
        var functionBodies = reader.ReadVarUInt32();

        if (functionBodies > 0 && (functionSignatures == null || functionSignatures.Length == importedFunctions))
            throw new ModuleLoadException("Code section is invalid when Function section is missing.", preBodiesIndex);
        if (functionBodies != functionSignatures.Length - importedFunctions)
            throw new ModuleLoadException($"Code section has {functionBodies} functions described but {functionSignatures.Length - importedFunctions} were expected.", preBodiesIndex);

        for (var functionBodyIndex = 0; functionBodyIndex < functionBodies; functionBodyIndex++)
        {
            var signature = functionSignatures[importedFunctions + functionBodyIndex];
            var byteLength = reader.ReadVarUInt32();
            var startingOffset = reader.Offset;

            var locals = new Local[reader.ReadVarUInt32()];
            for (var localIndex = 0; localIndex < locals.Length; localIndex++)
                locals[localIndex] = new Local(reader);

            var il = ((MethodBuilder)internalFunctions[importedFunctions + functionBodyIndex]).GetILGenerator();

            context.Reset(
                il,
                signature,
                [
                    .. signature.RawParameterTypes,
                    .. locals
                    .SelectMany(local => Enumerable.Range(0, checked((int)local.Count)).Select(_ => local.Type))
,
                ]);

            foreach (var local in locals.SelectMany(local => Enumerable.Range(0, checked((int)local.Count)).Select(_ => local.Type)))
            {
                il.DeclareLocal(local.ToSystemType());
            }

            foreach (var instruction in Instruction.Parse(reader))
            {
                instruction.Compile(context);
                context.Previous = instruction.OpCode;
            }

            if (reader.Offset - startingOffset != byteLength)
                throw new ModuleLoadException($"Instruction sequence reader ended after reading {reader.Offset - startingOffset} characters, expected {byteLength}.", reader.Offset);
        }
    }

    static void SectionData(Reader reader, CompilationContext context, FieldBuilder memory, ILGenerator instanceConstructorIL, TypeBuilder exportsBuilder)
    {
        var count = reader.ReadVarUInt32();

        context.Reset(
            instanceConstructorIL,
            Signature.Empty,
            Signature.Empty.RawParameterTypes
            );
        var block = new Instructions.Block(BlockType.Int32);

        var address = instanceConstructorIL.DeclareLocal(typeof(uint));

        for (var i = 0; i < count; i++)
        {
            var startingOffset = reader.Offset;
            {
                var index = reader.ReadVarUInt32();
                if (index != 0)
                    throw new ModuleLoadException($"Data index must be 0, found {index}.", startingOffset);
            }

            block.Compile(context); //Prevents "end" instruction of the initializer expression from becoming a return.
            foreach (var instruction in Instruction.ParseInitializerExpression(reader))
            {
                instruction.Compile(context);
                context.Previous = instruction.OpCode;
            }
            context.Stack.Pop();
            context.BlockContexts.Remove(context.Depth.Count);
            instanceConstructorIL.Emit(OpCodes.Stloc, address);

            var data = reader.ReadBytes(reader.ReadVarUInt32());

            if (data.Length == 0)
                continue;

            //Ensure sufficient memory is allocated, error if max is exceeded.
            instanceConstructorIL.Emit(OpCodes.Ldloc, address);
            instanceConstructorIL.Emit(OpCodes.Ldc_I4, data.Length);
            instanceConstructorIL.Emit(OpCodes.Add_Ovf_Un);

            instanceConstructorIL.Emit(OpCodes.Ldarg_0);

            instanceConstructorIL.Emit(OpCodes.Call, context[HelperMethod.RangeCheck8, Instructions.MemoryImmediateInstruction.CreateRangeCheck]);
            instanceConstructorIL.Emit(OpCodes.Pop);

            if (data.Length > 0x3f0000) //Limitation of DefineInitializedData, can be corrected by splitting the data.
                throw new NotSupportedException($"Data segment {i} is length {data.Length}, exceeding the current implementation limit of 4128768.");

            var field = exportsBuilder.DefineInitializedData($"☣ Data {i}", data, FieldAttributes.Assembly | FieldAttributes.InitOnly);

            instanceConstructorIL.Emit(OpCodes.Ldarg_0);
            instanceConstructorIL.Emit(OpCodes.Ldfld, memory);
            instanceConstructorIL.Emit(OpCodes.Call, UnmanagedMemory.StartGetter);
            instanceConstructorIL.Emit(OpCodes.Ldloc, address);
            instanceConstructorIL.Emit(OpCodes.Conv_I);
            instanceConstructorIL.Emit(OpCodes.Add_Ovf_Un);

            instanceConstructorIL.Emit(OpCodes.Ldsflda, field);

            instanceConstructorIL.Emit(OpCodes.Ldc_I4, data.Length);

            instanceConstructorIL.Emit(OpCodes.Cpblk);
        }
    }

    static FieldBuilder CreateFunctionTableField(TypeBuilder exportsBuilder, CompilerConfiguration configuration)
        => exportsBuilder.DefineField(
            "☣ FunctionTable",
            configuration.NeutralizeType(typeof(FunctionTable)),
            FieldAttributes.Private | FieldAttributes.InitOnly);

    static FieldBuilder CreateMemoryField(TypeBuilder exportsBuilder, CompilerConfiguration configuration)
        => exportsBuilder.DefineField(
            "☣ Memory",
            configuration.NeutralizeType(typeof(UnmanagedMemory)),
            PrivateReadonlyField);
}
