using System;
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
        uint? declaredDataCount = null;
        var delegateInvokersByTypeIndex = context.DelegateInvokersByTypeIndex;
        var delegateRemappersByType = context.DelegateRemappersByType;
        var emptyTypes = Type.EmptyTypes;
        var functionReferencesInitialized = false;

        // Lazily emits (once) the FunctionReferences array initialization into the instance constructor, before any
        // global/element population or function body that may consume it via ref.func or reference-typed segments.
        void EnsureFunctionReferences()
        {
            if (functionReferencesInitialized)
                return;
            functionReferencesInitialized = true;
            if (functionSignatures == null || functionSignatures.Length == 0)
                return;
            context.FunctionReferences = exportsBuilder.DefineField("☣ FunctionReferences", typeof(Delegate[]), PrivateReadonlyField);
            EmitFunctionReferencesInitialization(instanceConstructorIL, context.FunctionReferences, internalFunctions, functionSignatures, delegateInvokersByTypeIndex, configuration, exportsBuilder);
        }

        var preSectionOffset = reader.Offset;
        while (reader.TryReadVarUInt7(out var id)) //At points where TryRead is used, the stream can safely end.
        {
            if (id != 0 && ((Section)id).BinaryOrder < previousSection.BinaryOrder)
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
                                MultiValueHelper.ClrReturnType(signature.ReturnTypes),
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
                            if (elementType != ElementType.FunctionReference && elementType != ElementType.ExternRef)
                                throw new ModuleLoadException($"Unsupported table element type {elementType}.", preElementTypeOffset);

                            var limits = new ResizableLimits(reader);
                            var tableType = configuration.NeutralizeType(elementType == ElementType.FunctionReference ? typeof(FunctionTable) : typeof(ExternRefTable));
                            var tableField = exportsBuilder.DefineField($"☣ Table{context.Tables.Count}", tableType, FieldAttributes.Private | FieldAttributes.InitOnly);

                            context.Tables.Add(tableField);
                            context.TableElementTypes.Add(elementType);

                            // Retain the first funcref table for the WASM 1.0 single-table paths (call_indirect, active elements).
                            if (functionTable == null && elementType == ElementType.FunctionReference)
                                functionTable = tableField;

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
                                instanceConstructorIL.Emit(OpCodes.Newobj, tableType
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
                                instanceConstructorIL.Emit(OpCodes.Newobj, tableType
                                    .GetTypeInfo()
                                    .DeclaredConstructors
                                    .Where(constructor =>
                                    {
                                        var parms = constructor.GetParameters();
                                        return parms.Length == 1 && parms[0].ParameterType == configuration.NeutralizeType(typeof(uint));
                                    })
                                    .Single());
                            }
                            instanceConstructorIL.Emit(OpCodes.Stfld, tableField);
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
                    EnsureFunctionReferences(); // Global initializers may use ref.func.
                    globals = SectionGlobal(reader, context, globals, exportsBuilder, instanceConstructorIL, importedGlobals, configuration);
                    break;

                case Section.Export:
                    exportedFunctions = SectionExport(reader, functionTable, exportsBuilder, emptyTypes, memory, globals, configuration);
                    // Exported functions are implicitly declared as referenceable by ref.func.
                    foreach (var exported in exportedFunctions)
                        context.DeclaredFunctionReferences.Add(exported.Value);
                    break;

                case Section.Start:
                    if (internalFunctions == null)
                        throw new ModuleLoadException("Start section created without any functions.", preSectionOffset);
                    startFunction = SectionStart(reader, internalFunctions);
                    break;

                case Section.Element:
                    EnsureFunctionReferences(); // Element initializers reference functions via FunctionReferences.
                    SectionElement(reader, context, instanceConstructorIL, functionSignatures, internalFunctions, configuration, exportsBuilder);
                    break;

                case Section.Code:
                    if (functionSignatures == null)
                        throw new InvalidOperationException($"Code section found but {nameof(functionSignatures)} is null");
                    if (internalFunctions == null)
                        throw new InvalidOperationException($"Code section found but {nameof(internalFunctions)} is null");
                    EnsureFunctionReferences(); // Function bodies may use ref.func.
                    context.EnforceDeclaredFunctionReferences = true;
                    SectionCode(reader, context, functionSignatures, internalFunctions, importedFunctions);
                    break;

                case Section.DataCount:
                    {
                        // Pre-allocate a backing field for every data segment so memory.init / data.drop can
                        // reference passive segments during the Code section, which is compiled before the Data section.
                        declaredDataCount = reader.ReadVarUInt32();
                        for (var di = 0u; di < declaredDataCount; di++)
                        {
                            context.DataSegments[di] = exportsBuilder.DefineField(
                                $"☣ PassiveData {di}",
                                typeof(byte[]),
                                FieldAttributes.Private);
                        }
                    }
                    break;

                case Section.Data:
                    SectionData(reader, context, memory, instanceConstructorIL, exportsBuilder, declaredDataCount);
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
                    MultiValueHelper.ClrReturnType(signature.ReturnTypes),
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
                        // Two-or-more results map onto a delegate with a single (ValueTuple) return.
                        var del = configuration.GetDelegateForType(signature.ParameterTypes.Length, signature.ReturnTypes.Length > 1 ? 1 : signature.ReturnTypes.Length);
                        if (del == null)
                        {
                            missingDelegates.Add(new MissingDelegateType(moduleName, fieldName, signature));
                            continue;
                        }

                        var typedDelegate = configuration.NeutralizeType(del.IsGenericTypeDefinition ? del.MakeGenericType(MultiValueHelper.DelegateTypeArgs(signature.ParameterTypes, signature.ReturnTypes)) : del);
                        var delField = $"➡ {moduleName}::{fieldName}";
                        var delFieldBuilder = exportsBuilder.DefineField(delField, typedDelegate, PrivateReadonlyField);

                        var invoker = exportsBuilder.DefineMethod(
                            $"Invoke {delField}",
                            InternalFunctionAttributes,
                            CallingConventions.Standard,
                            MultiValueHelper.ClrReturnType(signature.ReturnTypes),
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
                        if (elementType != ElementType.FunctionReference && elementType != ElementType.ExternRef)
                            throw new ModuleLoadException($"{moduleName}::{fieldName} imported table element type {elementType} is not recognized.", preElementTypeoffset);

                        var limits = new ResizableLimits(reader);

                        var tableType = configuration.NeutralizeType(elementType == ElementType.FunctionReference ? typeof(FunctionTable) : typeof(ExternRefTable));
                        var tableField = exportsBuilder.DefineField($"☣ Table{context.Tables.Count}", tableType, FieldAttributes.Private | FieldAttributes.InitOnly);

                        context.Tables.Add(tableField);
                        context.TableElementTypes.Add(elementType);
                        if (functionTable == null && elementType == ElementType.FunctionReference)
                            functionTable = tableField;

                        instanceConstructorIL.Emit(OpCodes.Ldarg_0);
                        instanceConstructorIL.Emit(OpCodes.Ldarg_1);
                        instanceConstructorIL.Emit(OpCodes.Ldstr, moduleName);
                        instanceConstructorIL.Emit(OpCodes.Ldstr, fieldName);
                        instanceConstructorIL.Emit(OpCodes.Callvirt, importFinderInvoke);

                        ImportException.EmitTryCast(instanceConstructorIL, tableType, configuration);

                        instanceConstructorIL.Emit(OpCodes.Stfld, tableField);
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
                    if (instruction is Instructions.RefFunc rfGlobal)
                        context.DeclaredFunctionReferences.Add(rfGlobal.Index); // A ref.func in a global initializer declares that function.
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

                    if (instruction is Instructions.RefFunc rfGlobalMutable)
                        context.DeclaredFunctionReferences.Add(rfGlobalMutable.Index); // A ref.func in a global initializer declares that function.

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

    // Builds the per-function delegate array used by ref.func and reference-typed element segments, indexed by the
    // global function index (imported functions first, then internally-defined). Emitted into the instance constructor
    // before any element/global population that may reference it.
    static void EmitFunctionReferencesInitialization(ILGenerator il, FieldBuilder functionReferencesField, MethodInfo[]? internalFunctions, Signature[] functionSignatures, Dictionary<uint, MethodInfo> delegateInvokersByTypeIndex, CompilerConfiguration configuration, TypeBuilder exportsBuilder)
    {
        var total = functionSignatures.Length;
        var arrLocal = il.DeclareLocal(typeof(Delegate[]));
        il.EmitLoadConstant(total);
        il.Emit(OpCodes.Newarr, typeof(Delegate));
        il.Emit(OpCodes.Stloc, arrLocal);

        if (internalFunctions != null)
        {
            for (var i = 0; i < total && i < internalFunctions.Length; i++)
            {
                var method = internalFunctions[i];
                if (method == null)
                    continue;

                var signature = functionSignatures[i];
                var parms = signature.ParameterTypes;
                var returns = signature.ReturnTypes;

                if (!delegateInvokersByTypeIndex.TryGetValue(signature.TypeIndex, out var invoker))
                {
                    // Two-or-more results map onto a delegate with a single (ValueTuple) return.
                    var del = configuration.GetDelegateForType(parms.Length, returns.Length > 1 ? 1 : returns.Length);
                    if (del == null)
                        continue; // No standard delegate exists for this arity (e.g. more than 16 parameters), so the
                                  // function cannot be used as a reference; the array slot stays null. This is only
                                  // observable if such a function is actually referenced, which is itself unsupported.
                    if (del.IsGenericType)
                        del = del.MakeGenericType(MultiValueHelper.DelegateTypeArgs(parms, returns));

                    delegateInvokersByTypeIndex.Add(signature.TypeIndex, invoker = del.GetTypeInfo().GetDeclaredMethod(nameof(Action.Invoke))!);
                }

                var wrapper = exportsBuilder.DefineMethod(
                    $"↪ {i}",
                    MethodAttributes.Private | MethodAttributes.HideBySig,
                    MultiValueHelper.ClrReturnType(returns),
                    parms);
                var wil = wrapper.GetILGenerator();
                for (var k = 0; k < parms.Length; k++)
                    wil.EmitLoadArg(k + 1);
                wil.EmitLoadArg(0);
                wil.Emit(OpCodes.Call, method);
                wil.Emit(OpCodes.Ret);

                il.Emit(OpCodes.Ldloc, arrLocal);
                il.EmitLoadConstant(i);
                il.EmitLoadArg(0);
                il.Emit(OpCodes.Ldftn, wrapper);
                il.Emit(OpCodes.Newobj, invoker.DeclaringType!.GetTypeInfo().DeclaredConstructors.Single());
                il.Emit(OpCodes.Stelem_Ref);
            }
        }

        il.EmitLoadArg(0);
        il.Emit(OpCodes.Ldloc, arrLocal);
        il.Emit(OpCodes.Stfld, functionReferencesField);
    }

    // Emits IL that pushes a reference value onto the stack for an element entry: either a funcref delegate loaded from
    // FunctionReferences (and the function index registered as declared), or a null reference for ref.null.
    static void ValidateElementInitExpr(IList<Instruction> initExpr, ElementType elemType)
    {
        // A reference-type element initializer is a constant expression yielding exactly one reference value, then end.
        if (initExpr.Count != 2 || initExpr[1] is not Instructions.End)
            throw new ModuleLoadException("An element initializer expression must be a single reference-producing constant expression followed by end.", 0);

        switch (initExpr[0])
        {
            case Instructions.RefFunc when elemType == ElementType.FunctionReference:
                break;
            case Instructions.RefNull refNull when (ElementType)refNull.Type == elemType:
                break;
            // global.get's reference type isn't validated here; any other instruction yields a non-matching value.
            case Instructions.GlobalGet:
                break;
            default:
                throw new ModuleLoadException($"Element initializer expression ({initExpr[0].OpCode}) does not produce a value matching the segment element type {elemType}.", 0);
        }
    }

    static void EmitElementRefValue(ILGenerator il, CompilationContext context, IList<Instruction> initExpr)
    {
        // Each per-element initializer is a constant expression of the form [ref.func N, end] or [ref.null t, end].
        if (initExpr.Count >= 1 && initExpr[0] is Instructions.RefFunc refFunc)
        {
            context.DeclaredFunctionReferences.Add(refFunc.Index);
            il.EmitLoadArg(0);
            il.Emit(OpCodes.Ldfld, context.FunctionReferences!);
            il.EmitLoadConstant(refFunc.Index);
            il.Emit(OpCodes.Ldelem_Ref);
        }
        else
        {
            il.Emit(OpCodes.Ldnull);
        }
    }

    static void SectionElement(Reader reader, CompilationContext context, ILGenerator instanceConstructorIL, Signature[]? functionSignatures, MethodInfo[]? internalFunctions, CompilerConfiguration configuration, TypeBuilder exportsBuilder)
    {
        var count = reader.ReadVarUInt32();

        for (var i = 0u; i < count; i++)
        {
            var element = new Element(reader);
            var isActive = (element.Kind & 1) == 0;
            var isDeclarative = element.Kind is 3 or 7;
            var usesInitExprs = element.Kind >= 4;
            var elemType = element.Kind switch
            {
                0 or 1 or 2 or 3 => ElementType.FunctionReference,
                _ => element.ElemType,
            };
            var entryCount = usesInitExprs ? element.InitExprs.Count : element.Elements.Count;

            // Register declared function references (func-index forms); init-expr forms register in EmitElementRefValue.
            if (!usesInitExprs)
                foreach (var fi in element.Elements)
                    context.DeclaredFunctionReferences.Add(fi);

            // Register a backing field for every segment so elem.drop / table.init can resolve any valid index.
            // Passive segments are populated below; active and declarative segments leave the field null (= empty/dropped).
            var segField = exportsBuilder.DefineField(
                $"☣ ElementSegment {i}",
                elemType == ElementType.FunctionReference ? typeof(Delegate[]) : typeof(object[]),
                FieldAttributes.Private);
            context.ElementSegments[i] = segField;
            context.ElementSegmentTypes[i] = elemType;

            // Each reference-producing initializer expression must yield a value matching the segment's element type.
            if (usesInitExprs)
                foreach (var initExpr in element.InitExprs)
                    ValidateElementInitExpr(initExpr, elemType);

            if (isActive)
            {
                var tableIndex = element.Index;
                if (tableIndex >= (uint)context.Tables.Count)
                    throw new ModuleLoadException($"Element segment references table {tableIndex} but only {context.Tables.Count} tables exist.", 0);

                var tableElemType = context.GetTableElementType(tableIndex);
                if (tableElemType != elemType)
                    throw new ModuleLoadException($"Element segment type {elemType} does not match table {tableIndex} type {tableElemType}.", 0);

                // Offset from the initializer expression (single Int32 constant followed by end).
                var initializer = element.InitializerExpression;
                if (initializer.Count != 2 || initializer[0] is not Instructions.Int32Constant c || initializer[1] is not Instructions.End)
                    throw new ModuleLoadException("Element segment offset must be a single Int32 constant followed by end.", 0);
                var offset = (uint)c.Value;

                if (entryCount == 0)
                    continue;

                var isFunc = tableElemType == ElementType.FunctionReference;
                var tableType = isFunc ? typeof(FunctionTable) : typeof(ExternRefTable);
                var localTable = instanceConstructorIL.DeclareLocal(tableType);
                instanceConstructorIL.EmitLoadArg(0);
                instanceConstructorIL.Emit(OpCodes.Ldfld, context.GetTable(tableIndex));
                instanceConstructorIL.Emit(OpCodes.Stloc, localTable);

                var setter = isFunc ? FunctionTable.IndexSetter : ExternRefTable.IndexSetter;

                // Grow a funcref table that is too small to hold the segment (preserves the historical behavior of
                // sizing the table up to fit active elements rather than trapping).
                if (isFunc)
                {
                    var isBigEnough = instanceConstructorIL.DefineLabel();
                    instanceConstructorIL.Emit(OpCodes.Ldloc, localTable);
                    instanceConstructorIL.Emit(OpCodes.Call, FunctionTable.LengthGetter);
                    instanceConstructorIL.EmitLoadConstant(checked(offset + (uint)entryCount));
                    instanceConstructorIL.Emit(OpCodes.Bge_Un, isBigEnough);

                    instanceConstructorIL.Emit(OpCodes.Ldloc, localTable);
                    instanceConstructorIL.EmitLoadConstant(checked(offset + (uint)entryCount));
                    instanceConstructorIL.Emit(OpCodes.Ldloc, localTable);
                    instanceConstructorIL.Emit(OpCodes.Call, FunctionTable.LengthGetter);
                    instanceConstructorIL.Emit(OpCodes.Sub);
                    instanceConstructorIL.Emit(OpCodes.Call, FunctionTable.GrowMethod);
                    instanceConstructorIL.Emit(OpCodes.Pop);

                    instanceConstructorIL.MarkLabel(isBigEnough);
                }

                for (var j = 0; j < entryCount; j++)
                {
                    instanceConstructorIL.Emit(OpCodes.Ldloc, localTable);
                    instanceConstructorIL.EmitLoadConstant(offset + (uint)j);
                    if (usesInitExprs)
                        EmitElementRefValue(instanceConstructorIL, context, element.InitExprs[j]);
                    else
                        EmitFuncIndexRef(instanceConstructorIL, context, element.Elements[j]);
                    instanceConstructorIL.Emit(OpCodes.Call, setter);
                }
            }
            else if (isDeclarative)
            {
                // Declarative segments only declare references (handled above for func-index form; below for init-expr form);
                // they hold no runtime data and are considered already dropped.
                if (usesInitExprs)
                    foreach (var expr in element.InitExprs)
                        if (expr.Count >= 1 && expr[0] is Instructions.RefFunc rf)
                            context.DeclaredFunctionReferences.Add(rf.Index);
            }
            else
            {
                // Passive segment: populate the backing array field that table.init copies from and elem.drop nulls.
                var elementClrType = elemType == ElementType.FunctionReference ? typeof(Delegate) : typeof(object);
                context.PassiveElementSegments.Add(i);

                instanceConstructorIL.EmitLoadArg(0);
                instanceConstructorIL.EmitLoadConstant(entryCount);
                instanceConstructorIL.Emit(OpCodes.Newarr, elementClrType);
                for (var j = 0; j < entryCount; j++)
                {
                    instanceConstructorIL.Emit(OpCodes.Dup);
                    instanceConstructorIL.EmitLoadConstant(j);
                    if (usesInitExprs)
                        EmitElementRefValue(instanceConstructorIL, context, element.InitExprs[j]);
                    else
                        EmitFuncIndexRef(instanceConstructorIL, context, element.Elements[j]);
                    instanceConstructorIL.Emit(OpCodes.Stelem_Ref);
                }
                instanceConstructorIL.Emit(OpCodes.Stfld, segField);
            }
        }
    }

    // Emits IL that pushes the funcref delegate for a function index (from FunctionReferences) onto the stack.
    static void EmitFuncIndexRef(ILGenerator il, CompilationContext context, uint functionIndex)
    {
        context.DeclaredFunctionReferences.Add(functionIndex);
        il.EmitLoadArg(0);
        il.Emit(OpCodes.Ldfld, context.FunctionReferences!);
        il.EmitLoadConstant(functionIndex);
        il.Emit(OpCodes.Ldelem_Ref);
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

    static void SectionData(Reader reader, CompilationContext context, FieldBuilder? memory, ILGenerator instanceConstructorIL, TypeBuilder exportsBuilder, uint? declaredDataCount)
    {
        var count = reader.ReadVarUInt32();

        // When a DataCount section is present, its value must match the actual number of data segments.
        if (declaredDataCount != null && declaredDataCount != count)
            throw new ModuleLoadException($"data count and data section have inconsistent lengths: DataCount section declared {declaredDataCount} but the data section contains {count}.", reader.Offset);

        context.Reset(
            instanceConstructorIL,
            Signature.Empty,
            Signature.Empty.RawParameterTypes
            );
        var block = new Instructions.Block(BlockType.Int32);

        var address = instanceConstructorIL.DeclareLocal(typeof(uint));

        for (var i = 0u; i < count; i++)
        {
            var startingOffset = reader.Offset;
            var kind = reader.ReadVarUInt32();

            if (kind == 1)
            {
                StorePassiveDataSegment(context, instanceConstructorIL, exportsBuilder, i, reader.ReadBytes(reader.ReadVarUInt32()));
                continue;
            }

            // Kind 2 is active with an explicit memory index; only memory 0 is currently valid.
            if (kind == 2)
            {
                var memoryIndex = reader.ReadVarUInt32();
                if (memoryIndex != 0)
                    throw new ModuleLoadException($"Data memory index must be 0, found {memoryIndex}.", startingOffset);
            }
            else if (kind != 0)
            {
                throw new ModuleLoadException($"Data segment kind must be 0, 1, or 2, found {kind}.", startingOffset);
            }

            // Active data segments (kind 0 or 2) require a memory.
            if (memory == null)
                throw new ModuleLoadException("Active data segment cannot be used unless a memory section is defined.", startingOffset);

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

            //Ensure sufficient memory is allocated, error if max is exceeded. RangeCheck8 validates that the
            //last written byte (address + data.Length - 1) is accessible, which permits a segment that exactly
            //fills memory to the page boundary.
            instanceConstructorIL.Emit(OpCodes.Ldloc, address);
            instanceConstructorIL.Emit(OpCodes.Ldc_I4, data.Length - 1);
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

    // Initializes the byte[] field backing a passive data segment so memory.init can copy from it and data.drop can null it.
    static void StorePassiveDataSegment(CompilationContext context, ILGenerator instanceConstructorIL, TypeBuilder exportsBuilder, uint i, byte[] rawData)
    {
        // The field is pre-allocated when a DataCount section is present; allocate lazily when it is absent.
        if (!context.DataSegments.TryGetValue(i, out var segField))
        {
            segField = exportsBuilder.DefineField($"☣ PassiveData {i}", typeof(byte[]), FieldAttributes.Private);
            context.DataSegments[i] = segField;
        }

        if (rawData.Length == 0)
            return;

        if (rawData.Length > 0x3f0000) //Limitation of DefineInitializedData, can be corrected by splitting the data.
            throw new NotSupportedException($"Passive data segment {i} is length {rawData.Length}, exceeding the current implementation limit of 4128768.");

        // Use the same RVA-field + RuntimeHelpers.InitializeArray pattern that C# uses for array initializers:
        // create a new byte[], then bulk-initialize it from the PE's data, and store it into the instance field.
        var initField = exportsBuilder.DefineInitializedData($"☣ PassiveDataInit {i}", rawData, FieldAttributes.Assembly | FieldAttributes.InitOnly);

        instanceConstructorIL.Emit(OpCodes.Ldarg_0);
        instanceConstructorIL.Emit(OpCodes.Ldc_I4, rawData.Length);
        instanceConstructorIL.Emit(OpCodes.Newarr, typeof(byte));
        instanceConstructorIL.Emit(OpCodes.Dup);
        instanceConstructorIL.Emit(OpCodes.Ldtoken, initField);
        instanceConstructorIL.Emit(OpCodes.Call, RuntimeHelpersInitializeArray);
        instanceConstructorIL.Emit(OpCodes.Stfld, segField);
    }

    static readonly MethodInfo RuntimeHelpersInitializeArray = typeof(System.Runtime.CompilerServices.RuntimeHelpers)
        .GetMethod(nameof(System.Runtime.CompilerServices.RuntimeHelpers.InitializeArray), [typeof(Array), typeof(RuntimeFieldHandle)])!;

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
