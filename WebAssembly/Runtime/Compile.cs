using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
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
        Action? deferredElementWrites = null;
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
        bool functionRefsInitialized = false;

        // Create FunctionReferences field upfront (will be initialized in constructor if there are functions)
        context.FunctionReferences = exportsBuilder.DefineField(
            "☣ FunctionRefs",
            typeof(Delegate[]),
            FieldAttributes.Private | FieldAttributes.InitOnly);

        var preSectionOffset = reader.Offset;
        while (reader.TryReadVarUInt7(out var id)) //At points where TryRead is used, the stream can safely end.
        {
            var isDataCountBeforeCode = (Section)id == Section.DataCount && previousSection <= Section.Element;
            if (id != 0 && !isDataCountBeforeCode && (Section)id < previousSection)
                throw new ModuleLoadException($"Sections out of order; section {(Section)id} encountered after {previousSection}.", preSectionOffset);
            var payloadLength = reader.ReadVarUInt32();

            switch ((Section)id)
            {
                case Section.None:
                    {
                        var preNameOffset = reader.Offset;
                        reader.ReadString(reader.ReadVarUInt32()); //Name
                        reader.ReadBytes(payloadLength - checked((uint)(reader.Offset - preNameOffset))); //Content
                        preSectionOffset = reader.Offset;
                    }
                    continue;

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
                    ) = SectionImport(Import.ParseSection(reader), context, importedMemoryProvider, configuration, signatures, exportsBuilder, instanceConstructorIL, emptyTypes, memory, functionTable);
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
                                Compilation.MultiValueHelper.ClrReturnType(signature.ReturnTypes),
                                parms
                                );
                        }

                        // Initialize FunctionReferences immediately after Function section
                        // so that Global section (which comes next) can use ref.func in initializers
                        if (!functionRefsInitialized && context.FunctionReferences != null && functionSignatures != null && functionSignatures.Length > 0)
                        {
                            EmitFunctionReferencesInitialization(instanceConstructorIL, context.FunctionReferences, internalFunctions, functionSignatures, importedFunctions, configuration, exportsBuilder);
                            functionRefsInitialized = true;
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

                            context.TableElementTypes.Add(elementType);

                            var limits = new ResizableLimits(reader);
                            
                            // Create table field based on element type
                            FieldBuilder tableField;
                            Type tableType;
                            
                            if (elementType == ElementType.FunctionReference)
                            {
                                tableType = configuration.NeutralizeType(typeof(FunctionTable));
                                tableField = exportsBuilder.DefineField(
                                    $"☣ Table{i}",
                                    tableType,
                                    FieldAttributes.Private | FieldAttributes.InitOnly);
                            }
                            else // ElementType.ExternRef
                            {
                                tableType = configuration.NeutralizeType(typeof(ExternRefTable));
                                tableField = exportsBuilder.DefineField(
                                    $"☣ Table{i}",
                                    tableType,
                                    FieldAttributes.Private | FieldAttributes.InitOnly);
                            }
                            
                            context.Tables.Add(tableField);
                            
                            // Backward compatibility: first funcref table is also FunctionTable
                            if (functionTable == null && elementType == ElementType.FunctionReference)
                                functionTable = tableField;
                            
                            // Initialize table in constructor
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
                        var preMinOffset = reader.Offset;
                        memoryPagesMinimum = reader.ReadVarUInt32();
                        if (memoryPagesMinimum > 65536)
                            throw new ModuleLoadException($"Memory size must be at most 65536 pages (4GiB), but initial size is {memoryPagesMinimum}.", preMinOffset);
                        if ((setFlags & ResizableLimits.Flags.Maximum) != 0)
                        {
                            var preMaxOffset = reader.Offset;
                            var rawMax = reader.ReadVarUInt32();
                            if (rawMax > 65536)
                                throw new ModuleLoadException($"Memory size must be at most 65536 pages (4GiB), but maximum size is {rawMax}.", preMaxOffset);
                            if (memoryPagesMinimum > rawMax)
                                throw new ModuleLoadException($"Memory size minimum ({memoryPagesMinimum}) must not be greater than maximum ({rawMax}).", preMaxOffset);
                            memoryPagesMaximum = Math.Min(rawMax, uint.MaxValue / Memory.PageSize);
                        }
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
                    exportedFunctions = SectionExport(reader, context, functionTable, exportsBuilder, emptyTypes, memory, globals, configuration);
                    break;

                case Section.Start:
                    if (internalFunctions == null)
                        throw new ModuleLoadException("Start section created without any functions.", preSectionOffset);
                    startFunction = SectionStart(reader, internalFunctions);
                    break;

                case Section.Element:
                    deferredElementWrites = SectionElement(reader, functionTable, context, instanceConstructorIL, functionSignatures, internalFunctions, delegateInvokersByTypeIndex, configuration, exportsBuilder, preSectionOffset);
                    break;

                case Section.Code:
                    if (functionSignatures == null)
                        throw new InvalidOperationException($"Code section found but {nameof(functionSignatures)} is null");
                    if (internalFunctions == null)
                        throw new InvalidOperationException($"Code section found but {nameof(internalFunctions)} is null");
                    context.EnforceDeclaredFunctionReferences = true;
                    SectionCode(reader, context, functionSignatures, internalFunctions, importedFunctions);
                    break;

                case Section.Data:
                    SectionData(reader, context, memory, instanceConstructorIL, exportsBuilder, preSectionOffset);
                    
                    // Initialize FunctionReferences before element writes (if not already done)
                    if (!functionRefsInitialized && context.FunctionReferences != null)
                    {
                        if (functionSignatures != null)
                            EmitFunctionReferencesInitialization(instanceConstructorIL, context.FunctionReferences, internalFunctions, functionSignatures, importedFunctions, configuration, exportsBuilder);
                        else
                        {
                            // No functions, but still initialize empty array
                            instanceConstructorIL.EmitLoadArg(0);
                            instanceConstructorIL.EmitLoadConstant(0);
                            instanceConstructorIL.Emit(OpCodes.Newarr, typeof(Delegate));
                            instanceConstructorIL.Emit(OpCodes.Stfld, context.FunctionReferences);
                        }
                        functionRefsInitialized = true;
                    }
                    
                    deferredElementWrites?.Invoke(); // Emit element writes AFTER data bounds checks
                    deferredElementWrites = null;
                    break;

                case Section.DataCount:
                    {
                        // Pre-allocate passive data segment fields so memory.init / data.drop
                        // can reference them during SectionCode (which runs before SectionData).
                        var dataCount = reader.ReadVarUInt32();
                        for (var di = 0u; di < dataCount; di++)
                        {
                            var segField = exportsBuilder.DefineField(
                                $"☣ PassiveData {di}",
                                typeof(byte[]),
                                FieldAttributes.Private);
                            context.DataSegments[di] = segField;
                        }
                    }
                    break;

                default:
                    throw new ModuleLoadException($"Unrecognized section type {(Section)id}.", preSectionOffset);
            }

            preSectionOffset = reader.Offset;
            if ((Section)id != Section.DataCount)
                previousSection = (Section)id;
        }

        // Initialize FunctionReferences before element writes (if not already done and no Data section)
        if (!functionRefsInitialized && context.FunctionReferences != null)
        {
            if (functionSignatures != null)
                EmitFunctionReferencesInitialization(instanceConstructorIL, context.FunctionReferences, internalFunctions, functionSignatures, importedFunctions, configuration, exportsBuilder);
            else
            {
                // No functions, but still initialize empty array to avoid NullReferenceException
                instanceConstructorIL.EmitLoadArg(0);
                instanceConstructorIL.EmitLoadConstant(0);
                instanceConstructorIL.Emit(OpCodes.Newarr, typeof(Delegate));
                instanceConstructorIL.Emit(OpCodes.Stfld, context.FunctionReferences);
            }
            functionRefsInitialized = true;
        }

        // If there was no data section, element writes are still pending.
        deferredElementWrites?.Invoke();

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
                    Compilation.MultiValueHelper.ClrReturnType(signature.ReturnTypes),
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

        // Ensure FunctionReferences is initialized even if empty (to avoid NullReferenceException)
        if (!functionRefsInitialized && context.FunctionReferences != null)
        {
            instanceConstructorIL.Emit(OpCodes.Ldarg_0);
            instanceConstructorIL.Emit(OpCodes.Ldc_I4_0);
            instanceConstructorIL.Emit(OpCodes.Newarr, typeof(System.Delegate));
            instanceConstructorIL.Emit(OpCodes.Stfld, context.FunctionReferences);
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
    ) SectionImport(List<Import> imports, CompilationContext context, MethodBuilder? importedMemoryProvider, CompilerConfiguration configuration, Signature[]? signatures, TypeBuilder exportsBuilder, ILGenerator instanceConstructorIL, Type[] emptyTypes, FieldBuilder? memory, FieldBuilder? functionTable)
    {
        var functionImports = new List<MethodInfo>(imports.Count);
        var functionImportTypes = new List<Signature>(imports.Count);
        var globalImports = new List<GlobalInfo>(imports.Count);
        var missingDelegates = new List<MissingDelegateType>();
        var importFinderInvoke = configuration.NeutralizeType(typeof(Func<string, string, RuntimeImport>)).GetMethod("Invoke")!;

        foreach (var import in imports)
        {
            var moduleName = import.Module;
            var fieldName = import.Field;

            switch (import)
            {
                case Import.Function functionImport:
                    {
                        var typeIndex = functionImport.TypeIndex;
                        if (signatures == null)
                            throw new InvalidOperationException();
                        if (typeIndex >= signatures.Length)
                            throw new ModuleLoadException($"Requested type index {typeIndex} but only {signatures.Length} are available.", 0);
                        var signature = signatures[typeIndex];
                        var clrReturnCount = signature.ReturnTypes.Length > 1 ? 1 : signature.ReturnTypes.Length;
                        var del = configuration.GetDelegateForType(signature.ParameterTypes.Length, clrReturnCount);
                        if (del == null)
                        {
                            missingDelegates.Add(new MissingDelegateType(moduleName, fieldName, signature));
                            continue;
                        }

                        var delegateTypeArgs = Compilation.MultiValueHelper.DelegateTypeArgs(signature.ParameterTypes, signature.ReturnTypes);
                        var typedDelegate = configuration.NeutralizeType(del.IsGenericTypeDefinition ? del.MakeGenericType(delegateTypeArgs) : del);
                        var delField = $"➡ {moduleName}::{fieldName}";
                        var delFieldBuilder = exportsBuilder.DefineField(delField, typedDelegate, PrivateReadonlyField);

                        var invoker = exportsBuilder.DefineMethod(
                            $"Invoke {delField}",
                            InternalFunctionAttributes,
                            CallingConventions.Standard,
                            Compilation.MultiValueHelper.ClrReturnType(signature.ReturnTypes),
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
                case Import.Table tableImport:
                    {
                        var definition = tableImport.Definition ?? throw new ModuleLoadException($"Imported table {moduleName}::{fieldName} is missing its definition.", 0);
                        var elementType = definition.ElementType;
                        if (elementType != ElementType.FunctionReference && elementType != ElementType.ExternRef)
                            throw new ModuleLoadException($"{moduleName}::{fieldName} imported table type of  kind of {elementType} is not recognized.", 0);

                        context.TableElementTypes.Add(elementType);
                        var limits = definition.ResizableLimits;
                        var tableType = elementType == ElementType.FunctionReference ? typeof(FunctionTable) : typeof(ExternRefTable);
                        var tableField = exportsBuilder.DefineField(
                            $"☣ Table{context.Tables.Count}",
                            configuration.NeutralizeType(tableType),
                            PrivateReadonlyField);
                        context.Tables.Add(tableField);
                        if (functionTable == null && elementType == ElementType.FunctionReference)
                            functionTable = tableField;

                        instanceConstructorIL.Emit(OpCodes.Ldarg_0);
                        instanceConstructorIL.Emit(OpCodes.Ldarg_1);
                        instanceConstructorIL.Emit(OpCodes.Ldstr, moduleName);
                        instanceConstructorIL.Emit(OpCodes.Ldstr, fieldName);
                        instanceConstructorIL.Emit(OpCodes.Callvirt, importFinderInvoke);

                        ImportException.EmitTryCast(instanceConstructorIL, configuration.NeutralizeType(tableType), configuration);

                        instanceConstructorIL.Emit(OpCodes.Stfld, tableField);

                        // Validate limits: provided table must have initial >= required min, and max <= required max.
                        instanceConstructorIL.Emit(OpCodes.Ldarg_0);
                        instanceConstructorIL.Emit(OpCodes.Ldfld, tableField);
                        instanceConstructorIL.Emit(OpCodes.Ldc_I4, (int)limits.Minimum);
                        instanceConstructorIL.Emit(OpCodes.Ldc_I4, limits.Maximum.HasValue ? (int)limits.Maximum.Value : unchecked((int)uint.MaxValue));
                        instanceConstructorIL.Emit(OpCodes.Call, typeof(ImportException).GetMethod(nameof(ImportException.ValidateTableLimits), [tableType, typeof(uint), typeof(uint)])!);
                    }
                    break;
                case Import.Memory memoryImport:
                    {
                        var type = memoryImport.Type ?? throw new ModuleLoadException($"Imported memory {moduleName}::{fieldName} is missing its definition.", 0);
                        var limits = type.ResizableLimits;

                        if (memory != null)
                            throw new ModuleLoadException($"Multiple memories are not supported; encountered second memory import {moduleName}::{fieldName}.", 0);

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

                        // Validate limits: provided memory must have minimum >= required min, and max <= required max.
                        instanceConstructorIL.Emit(OpCodes.Ldarg_0);
                        instanceConstructorIL.Emit(OpCodes.Ldfld, memory);
                        instanceConstructorIL.Emit(OpCodes.Ldc_I4, (int)limits.Minimum);
                        instanceConstructorIL.Emit(OpCodes.Ldc_I4, limits.Maximum.HasValue ? (int)limits.Maximum.Value : unchecked((int)uint.MaxValue));
                        instanceConstructorIL.Emit(OpCodes.Call, typeof(ImportException).GetMethod(nameof(ImportException.ValidateMemoryLimits))!);
                    }
                    break;
                case Import.Global globalImport:
                    {
                        var contentType = globalImport.ContentType;
                        var mutable = globalImport.IsMutable;

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

                        // Validate mutability: throw ImportException if the provided global's mutability doesn't match.
                        instanceConstructorIL.Emit(OpCodes.Dup);
                        instanceConstructorIL.Emit(OpCodes.Ldc_I4, mutable ? 1 : 0);
                        instanceConstructorIL.Emit(OpCodes.Call, typeof(ImportException).GetMethod(nameof(ImportException.ValidateGlobalMutability))!);

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
                    throw new ModuleLoadException($"{moduleName}::{fieldName} imported external kind of {import.Kind} is not recognized.", 0);
            }
        }

        if (missingDelegates.Count != 0)
            throw new MissingDelegateTypesException(missingDelegates);

        context.Methods = [.. functionImports];
        context.FunctionSignatures = [.. functionImportTypes];
        context.Globals = [.. globalImports];
        context.ImportedGlobalCount = globalImports.Count;

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
                [exportsBuilder]
                );

            var il = getter.GetILGenerator();
            MethodBuilder? setter;

            {
                // Always use a backing field so init expressions can reference imported globals.
                var field = exportsBuilder.DefineField(
                    $"🌍 {i}",
                    configuration.NeutralizeType(contentType.ToSystemType()),
                    FieldAttributes.Private | (isMutable ? 0 : FieldAttributes.InitOnly)
                    );

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, field);
                il.Emit(OpCodes.Ret);

                if (isMutable)
                {
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
                }
                else
                {
                    setter = null;
                }

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
                        // Validate that the init expression produced exactly one value of the declared type.
                        if (context.Stack.Count != 1 || !context.Stack.Peek().Equals(contentType))
                            throw new StackTypeInvalidException(instruction.OpCode, contentType, context.Stack.Count > 0 ? context.Stack.Peek() : WebAssemblyValueType.Int32);
                        context.Emit(OpCodes.Stfld, field);
                        ended = true;
                        continue;
                    }

                    if (instruction is Instructions.GlobalGet ggInit)
                    {
                        if (ggInit.Index >= (uint)context.ImportedGlobalCount)
                            throw new ModuleLoadException("unknown global", reader.Offset);
                        if (context.Globals != null && ggInit.Index < (uint)context.Globals.Length && context.Globals[ggInit.Index].Setter != null)
                            throw new ModuleLoadException("constant expression required", reader.Offset);
                    }
                    else if (instruction is Instructions.RefFunc rfInit)
                    {
                        context.DeclaredFunctionReferences.Add(rfInit.Index);
                    }

                    instruction.Compile(context);
                    context.Previous = instruction.OpCode;
                }
            }

            globals[importedGlobals + i] = new GlobalInfo(contentType, true, getter, setter);
        }

        return globals;
    }

    static KeyValuePair<string, uint>[] SectionExport(
        Reader reader,
        CompilationContext context,
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
        var exportNames = new HashSet<string>();

        for (var i = 0; i < totalExports; i++)
        {
            var preNameOffset = reader.Offset;
            var name = reader.ReadString(reader.ReadVarUInt32());
            if (!exportNames.Add(name))
                throw new ModuleLoadException($"Duplicate export name \"{name}\".", preNameOffset);
            var preKindOffset = reader.Offset;
            var kind = (ExternalKind)reader.ReadByte();
            var preIndexOffset = reader.Offset;
            var index = reader.ReadVarUInt32();
            switch (kind)
            {
                case ExternalKind.Function:
                    xFunctions.Add(new KeyValuePair<string, uint>(name, index));
                    context.DeclaredFunctionReferences.Add(index);
                    break;
                case ExternalKind.Table:
                    if (index >= (uint)context.Tables.Count)
                        throw new ModuleLoadException("Can't export a table without defining or importing one.", preKindOffset);

                    {
                        var tableField = context.Tables[(int)index];
                        var tableElementType = context.TableElementTypes[(int)index];
                        var tableType = tableElementType == ElementType.FunctionReference ? typeof(FunctionTable) : typeof(ExternRefTable);
                        var tableGetter = exportsBuilder.DefineMethod("get_" + name,
                            exportedPropertyAttributes,
                            CallingConventions.HasThis,
                            configuration.NeutralizeType(tableType),
                            emptyTypes
                            );
#if NET9_0_OR_GREATER
                        if (configuration is not PersistedCompilerConfiguration) // Need to redesign this for persisted.
#endif
                        tableGetter.SetCustomAttribute(NativeExportAttribute.Emit(ExternalKind.Table, name));
                        var getterIL = tableGetter.GetILGenerator();
                        getterIL.Emit(OpCodes.Ldarg_0);
                        getterIL.Emit(OpCodes.Ldfld, tableField);
                        getterIL.Emit(OpCodes.Ret);

                        exportsBuilder.DefineProperty(name, PropertyAttributes.None, configuration.NeutralizeType(tableType), emptyTypes)
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

    // Returns an Action that emits the element-write IL. The caller must invoke it AFTER all
    // section checks (including data-segment bounds checks) have been emitted, to ensure that a
    // failed instantiation does not partially modify a shared imported table.
    static Action? SectionElement(Reader reader, FieldBuilder? functionTable, CompilationContext context, ILGenerator instanceConstructorIL, Signature[]? functionSignatures, MethodInfo[]? internalFunctions, Dictionary<uint, MethodInfo> delegateInvokersByTypeIndex, CompilerConfiguration configuration, TypeBuilder exportsBuilder, long sectionOffset = 0)
    {
        var count = reader.ReadVarUInt32();

        if (count == 0)
            return null;

        // Only load the function table local when there are active segments that need it.
        LocalBuilder? localFunctionTable = null;
        var setter = FunctionTable.IndexSetter;

        // Buffer active segment data so all bounds checks can be emitted before any writes.
        // This ensures atomicity: if any segment doesn't fit, no writes occur.
        // (tableIndex, constOffset, isDynamicOffset, globalIndex, funcIndices, dynamicOffsetLocal)
        var activeSegments = new System.Collections.Generic.List<(uint TableIndex, uint? ConstOffset, bool IsDynamic, uint GlobalIndex, uint[] FuncIndices, LocalBuilder? DynLocal)>();
        var activeGlobalRefSegments = new System.Collections.Generic.List<(uint TableIndex, uint? ConstOffset, bool IsDynamic, uint GlobalIndex, (byte Kind, uint Index)[] Entries, LocalBuilder? DynLocal)>();
        var activeExternRefSegments = new System.Collections.Generic.List<(uint TableIndex, uint? ConstOffset, bool IsDynamic, uint GlobalIndex, uint Length, LocalBuilder? DynLocal)>();
        
        // Buffer passive segment data to defer IL emission until after Code section.
        // (segmentIndex, segField, funcIndices (uint.MaxValue = null slot), elemType)
        var passiveSegments = new System.Collections.Generic.List<(int Index, FieldBuilder SegField, uint[] FuncIndices, ElementType ElemType)>();

        for (var i = 0; i < count; i++)
        {
            var kind = reader.ReadVarUInt32();

            // Kind 1: passive element segment — store function references for use by table.init.
            if (kind == 1)
            {
                reader.ReadByte(); // elemkind (always 0x00 = funcref)
                var elemCount = reader.ReadVarUInt32();
                var segField = exportsBuilder.DefineField(
                    $"☣ PassiveElem {i}",
                    typeof(Delegate[]),
                    FieldAttributes.Private);
                context.ElementSegments[(uint)i] = segField;
                context.ElementSegmentTypes[(uint)i] = ElementType.FunctionReference;

                if (elemCount > 0)
                {
                    var funcIndices = new uint[elemCount];
                    for (var j = 0u; j < elemCount; j++)
                    {
                        var idx = reader.ReadVarUInt32();
                        if (functionSignatures == null || idx >= (uint)functionSignatures.Length)
                            throw new ModuleLoadException($"Element segment {i}: function index {idx} is unknown.", reader.Offset);
                        funcIndices[j] = idx;
                        context.DeclaredFunctionReferences.Add(idx);
                    }

                    if (functionSignatures != null && internalFunctions != null)
                        passiveSegments.Add((i, segField, funcIndices, ElementType.FunctionReference));
                }
                continue;
            }

            // Kind 5: passive element segment with init-exprs (each is [ref.func idx, end] or [ref.null type, end]).
            if (kind == 5)
            {
                var segRefType = (ElementType)reader.ReadVarInt7();
                var elemCount = reader.ReadVarUInt32();
                var segField = exportsBuilder.DefineField(
                    $"☣ PassiveElem {i}",
                    typeof(Delegate[]),
                    FieldAttributes.Private);
                context.ElementSegments[(uint)i] = segField;
                context.ElementSegmentTypes[(uint)i] = segRefType;

                if (elemCount > 0 && functionSignatures != null && internalFunctions != null)
                {
                    // Parse all init-exprs first (uint.MaxValue = ref.null slot).
                    var funcIndices = new uint[elemCount];
                    for (var j = 0u; j < elemCount; j++)
                    {
                        var expr = Instruction.ParseInitializerExpression(reader).ToArray();
                        if (expr.Length == 2 && expr[0] is Instructions.RefFunc rf)
                        {
                            if (rf.Index >= (uint)functionSignatures.Length)
                                throw new ModuleLoadException($"Element segment {i}: function index {rf.Index} is unknown.", reader.Offset);
                            funcIndices[j] = rf.Index;
                            context.DeclaredFunctionReferences.Add(rf.Index);
                        }
                        else if (expr.Length == 2 && expr[0] is Instructions.RefNull)
                            funcIndices[j] = uint.MaxValue; // null slot
                        else
                            throw new ModuleLoadException($"Kind-5 element segment {i}: unsupported init expression.", reader.Offset);
                    }

                    passiveSegments.Add((i, segField, funcIndices, segRefType));
                }
                continue;
            }

            // Kind 4: active, table 0, init-exprs. Parse offset, validate and populate.
            if (kind == 4)
            {
                var preKind4Offset = reader.Offset;
                var kind4Initializer = Instruction.ParseInitializerExpression(reader).ToArray();
                uint? constOffset4 = null;
                uint globalIndex4 = 0;
                bool isDynamic4 = false;
                if (kind4Initializer.Length == 2 && kind4Initializer[0] is Instructions.Int32Constant ic4 && kind4Initializer[1] is Instructions.End)
                    constOffset4 = (uint)ic4.Value;
                else if (kind4Initializer.Length == 2 && kind4Initializer[0] is Instructions.GlobalGet gg4 && kind4Initializer[1] is Instructions.End)
                {
                    if (gg4.Index >= (uint)context.ImportedGlobalCount)
                        throw new ModuleLoadException("unknown global", preKind4Offset);
                    if (context.Globals != null && gg4.Index < (uint)context.Globals.Length && context.Globals[gg4.Index].Setter != null)
                        throw new ModuleLoadException("constant expression required", preKind4Offset);
                    globalIndex4 = gg4.Index;
                    isDynamic4 = true;
                }
                else
                    throw new ModuleLoadException("Initializer expression support for the Element section is limited to a single Int32 constant or global.get followed by end.", preKind4Offset);

                var tableElemType = context.TableElementTypes.Count > 0 ? context.TableElementTypes[0] : ElementType.FunctionReference;
                var exprCount = reader.ReadVarUInt32();
                var funcIndices4 = new uint[exprCount];
                var globalRefEntries4 = new (byte Kind, uint Index)[exprCount];
                var hasGlobalRefEntry4 = false;
                for (var j = 0u; j < exprCount; j++)
                {
                    var preExprOffset = reader.Offset;
                    var expr = Instruction.ParseInitializerExpression(reader).ToArray();
                    if (expr.Length != 2)
                        throw new ModuleLoadException("type mismatch", preExprOffset);
                    if (expr[0] is Instructions.RefFunc rf4)
                    {
                        if (tableElemType != ElementType.FunctionReference)
                            throw new ModuleLoadException("type mismatch", preExprOffset);
                        if (functionSignatures == null || rf4.Index >= (uint)functionSignatures.Length)
                            throw new ModuleLoadException($"Element segment {i}: function index {rf4.Index} is unknown.", preExprOffset);
                        funcIndices4[j] = rf4.Index;
                        globalRefEntries4[j] = (1, rf4.Index);
                        context.DeclaredFunctionReferences.Add(rf4.Index);
                    }
                    else if (expr[0] is Instructions.RefNull rn4)
                    {
                        var exprElemType = rn4.Type == WebAssemblyValueType.FuncRef ? ElementType.FunctionReference : ElementType.ExternRef;
                        if (exprElemType != tableElemType)
                            throw new ModuleLoadException("type mismatch", preExprOffset);
                        funcIndices4[j] = uint.MaxValue; // null slot
                        globalRefEntries4[j] = (0, 0);
                    }
                    else if (expr[0] is Instructions.GlobalGet gg4)
                    {
                        if (tableElemType != ElementType.FunctionReference)
                            throw new ModuleLoadException("type mismatch", preExprOffset);
                        if (gg4.Index >= (uint)context.ImportedGlobalCount)
                            throw new ModuleLoadException("unknown global", preExprOffset);
                        var globals4 = context.Globals ?? throw new CompilerException("global.get requires a global section.");
                        if (gg4.Index >= (uint)globals4.Length || globals4[gg4.Index].Setter != null)
                            throw new ModuleLoadException("constant expression required", preExprOffset);
                        if (globals4[gg4.Index].Type != WebAssemblyValueType.FuncRef)
                            throw new ModuleLoadException("type mismatch", preExprOffset);
                        funcIndices4[j] = uint.MaxValue;
                        globalRefEntries4[j] = (2, gg4.Index);
                        hasGlobalRefEntry4 = true;
                    }
                    else
                        throw new ModuleLoadException("type mismatch", preExprOffset);
                }

                if (exprCount > 0 && functionTable == null)
                    throw new ModuleLoadException("Active element segment requires a table section or import.", preKind4Offset);

                if (exprCount > 0)
                {
                    if (localFunctionTable == null)
                    {
                        localFunctionTable = instanceConstructorIL.DeclareLocal(typeof(FunctionTable));
                        instanceConstructorIL.EmitLoadArg(0);
                        instanceConstructorIL.Emit(OpCodes.Ldfld, functionTable!);
                        instanceConstructorIL.Emit(OpCodes.Stloc, localFunctionTable);
                    }

                    LocalBuilder? dynamicOffsetLocal4 = null;
                    if (isDynamic4)
                    {
                        dynamicOffsetLocal4 = instanceConstructorIL.DeclareLocal(typeof(uint));
                        var global4 = (context.Globals ?? throw new CompilerException("global.get requires a global section."))[globalIndex4];
                        if (global4.RequiresInstance)
                            instanceConstructorIL.EmitLoadArg(0);
                        instanceConstructorIL.Emit(OpCodes.Call, global4.Getter);
                        instanceConstructorIL.Emit(OpCodes.Stloc, dynamicOffsetLocal4);
                    }

                    context.ElementSegments[(uint)i] = exportsBuilder.DefineField($"☣ ActiveElem {i}", typeof(Delegate[]), FieldAttributes.Private);
                    context.ElementSegmentTypes[(uint)i] = ElementType.FunctionReference;
                    if (hasGlobalRefEntry4)
                        activeGlobalRefSegments.Add((0, constOffset4, isDynamic4, globalIndex4, globalRefEntries4, dynamicOffsetLocal4));
                    else
                        activeSegments.Add((0, constOffset4, isDynamic4, globalIndex4, funcIndices4, dynamicOffsetLocal4));
                }
                continue;
            }

            // Kind 6: active, explicit table index, init-exprs.
            if (kind == 6)
            {
                var preKind6Offset = reader.Offset;
                var tableIdx = reader.ReadVarUInt32();
                var kind6Initializer = Instruction.ParseInitializerExpression(reader).ToArray();
                var reftype = (ElementType)reader.ReadVarInt7();
                var targetElemType = tableIdx < (uint)context.TableElementTypes.Count ? context.TableElementTypes[(int)tableIdx] : ElementType.FunctionReference;
                if (reftype != targetElemType)
                    throw new ModuleLoadException("type mismatch", preKind6Offset);

                uint? constOffset6 = null;
                uint globalIndex6 = 0;
                bool isDynamic6 = false;
                if (kind6Initializer.Length == 2 && kind6Initializer[0] is Instructions.Int32Constant ic6 && kind6Initializer[1] is Instructions.End)
                    constOffset6 = (uint)ic6.Value;
                else if (kind6Initializer.Length == 2 && kind6Initializer[0] is Instructions.GlobalGet gg6 && kind6Initializer[1] is Instructions.End)
                {
                    if (gg6.Index >= (uint)context.ImportedGlobalCount)
                        throw new ModuleLoadException("unknown global", preKind6Offset);
                    if (context.Globals != null && gg6.Index < (uint)context.Globals.Length && context.Globals[gg6.Index].Setter != null)
                        throw new ModuleLoadException("constant expression required", preKind6Offset);
                    globalIndex6 = gg6.Index;
                    isDynamic6 = true;
                }
                else
                    throw new ModuleLoadException("Initializer expression support for the Element section is limited to a single Int32 constant or global.get followed by end.", preKind6Offset);

                var exprCount6 = reader.ReadVarUInt32();
                var funcIndices6 = new uint[exprCount6];
                var globalRefEntries6 = new (byte Kind, uint Index)[exprCount6];
                uint externRefNullCount6 = 0;
                var hasGlobalRefEntry6 = false;
                for (var j = 0u; j < exprCount6; j++)
                {
                    var preExprOffset = reader.Offset;
                    var expr = Instruction.ParseInitializerExpression(reader).ToArray();
                    if (expr.Length != 2)
                        throw new ModuleLoadException("type mismatch", preExprOffset);
                    if (expr[0] is Instructions.RefFunc rf6)
                    {
                        if (reftype != ElementType.FunctionReference)
                            throw new ModuleLoadException("type mismatch", preExprOffset);
                        if (functionSignatures == null || rf6.Index >= (uint)functionSignatures.Length)
                            throw new ModuleLoadException($"Element segment {i}: function index {rf6.Index} is unknown.", preExprOffset);
                        funcIndices6[j] = rf6.Index;
                        globalRefEntries6[j] = (1, rf6.Index);
                        context.DeclaredFunctionReferences.Add(rf6.Index);
                    }
                    else if (expr[0] is Instructions.RefNull rn6)
                    {
                        var exprElemType = rn6.Type == WebAssemblyValueType.FuncRef ? ElementType.FunctionReference : ElementType.ExternRef;
                        if (exprElemType != reftype)
                            throw new ModuleLoadException("type mismatch", preExprOffset);
                        if (reftype == ElementType.FunctionReference)
                        {
                            funcIndices6[j] = uint.MaxValue;
                            globalRefEntries6[j] = (0, 0);
                        }
                        else
                            externRefNullCount6++;
                    }
                    else if (expr[0] is Instructions.GlobalGet gg6)
                    {
                        if (reftype != ElementType.FunctionReference)
                            throw new ModuleLoadException("type mismatch", preExprOffset);
                        if (gg6.Index >= (uint)context.ImportedGlobalCount)
                            throw new ModuleLoadException("unknown global", preExprOffset);
                        var globals6 = context.Globals ?? throw new CompilerException("global.get requires a global section.");
                        if (gg6.Index >= (uint)globals6.Length || globals6[gg6.Index].Setter != null)
                            throw new ModuleLoadException("constant expression required", preExprOffset);
                        if (globals6[gg6.Index].Type != WebAssemblyValueType.FuncRef)
                            throw new ModuleLoadException("type mismatch", preExprOffset);
                        funcIndices6[j] = uint.MaxValue;
                        globalRefEntries6[j] = (2, gg6.Index);
                        hasGlobalRefEntry6 = true;
                    }
                    else
                        throw new ModuleLoadException("type mismatch", preExprOffset);
                }

                LocalBuilder? dynamicOffsetLocal6 = null;
                if (isDynamic6)
                {
                    dynamicOffsetLocal6 = instanceConstructorIL.DeclareLocal(typeof(uint));
                    var global6 = (context.Globals ?? throw new CompilerException("global.get requires a global section."))[globalIndex6];
                    if (global6.RequiresInstance)
                        instanceConstructorIL.EmitLoadArg(0);
                    instanceConstructorIL.Emit(OpCodes.Call, global6.Getter);
                    instanceConstructorIL.Emit(OpCodes.Stloc, dynamicOffsetLocal6);
                }

                context.ElementSegments[(uint)i] = exportsBuilder.DefineField(
                    $"☣ ActiveElem {i}",
                    reftype == ElementType.FunctionReference ? typeof(Delegate[]) : typeof(object[]),
                    FieldAttributes.Private);
                context.ElementSegmentTypes[(uint)i] = reftype;
                if (reftype == ElementType.FunctionReference)
                {
                    if (hasGlobalRefEntry6)
                        activeGlobalRefSegments.Add((tableIdx, constOffset6, isDynamic6, globalIndex6, globalRefEntries6, dynamicOffsetLocal6));
                    else
                        activeSegments.Add((tableIdx, constOffset6, isDynamic6, globalIndex6, funcIndices6, dynamicOffsetLocal6));
                }
                else
                    activeExternRefSegments.Add((tableIdx, constOffset6, isDynamic6, globalIndex6, externRefNullCount6, dynamicOffsetLocal6));
                continue;
            }

            // Kind 2: active, explicit table index, offset expr, elemkind, func indices.
            if (kind == 2)
            {
                var preKind2Offset = reader.Offset;
                var tableIdx2 = reader.ReadVarUInt32();

                if (tableIdx2 >= (uint)context.TableElementTypes.Count)
                    throw new ModuleLoadException($"Table index {tableIdx2} out of range.", preKind2Offset);

                context.ElementSegments[(uint)i] = exportsBuilder.DefineField(
                    $"☣ ActiveElem {i}",
                    typeof(Delegate[]),
                    FieldAttributes.Private);
                context.ElementSegmentTypes[(uint)i] = ElementType.FunctionReference;

                uint? constOffset2 = null;
                uint globalIndex2 = 0;
                bool isDynamicOffset2 = false;
                {
                    var preInitializerOffset = reader.Offset;
                    var initializer2 = Instruction.ParseInitializerExpression(reader).ToArray();
                    if (initializer2.Length == 2 && initializer2[0] is Instructions.Int32Constant ic2 && initializer2[1] is Instructions.End)
                    {
                        constOffset2 = (uint)ic2.Value;
                    }
                    else if (initializer2.Length == 2 && initializer2[0] is Instructions.GlobalGet gg2 && initializer2[1] is Instructions.End)
                    {
                        if (gg2.Index >= (uint)context.ImportedGlobalCount)
                            throw new ModuleLoadException("unknown global", preInitializerOffset);
                        if (context.Globals != null && gg2.Index < (uint)context.Globals.Length && context.Globals[gg2.Index].Setter != null)
                            throw new ModuleLoadException("constant expression required", preInitializerOffset);
                        globalIndex2 = gg2.Index;
                        isDynamicOffset2 = true;
                    }
                    else
                    {
                        throw new ModuleLoadException("Initializer expression support for the Element section is limited to a single Int32 constant or global.get followed by end.", preInitializerOffset);
                    }
                }

                reader.ReadByte(); // elemkind (0x00 = funcref)
                
                var elements2 = reader.ReadVarUInt32();
                var funcIndicesArr2 = new uint[elements2];
                if (elements2 > 0)
                {
                    if (functionSignatures == null || internalFunctions == null)
                        throw new ModuleLoadException("Element section must be empty if there are no functions to reference.", reader.Offset);
                    for (var j = 0u; j < elements2; j++)
                    {
                        var funcIdx = reader.ReadVarUInt32();
                        if (funcIdx >= (uint)functionSignatures.Length)
                            throw new ModuleLoadException($"Element segment {i}: function index {funcIdx} is unknown.", reader.Offset);
                        funcIndicesArr2[j] = funcIdx;
                        context.DeclaredFunctionReferences.Add(funcIdx);
                    }
                }

                // For dynamic offsets (global.get), emit IL to load the global value into a local.
                LocalBuilder? dynamicOffsetLocal2 = null;
                if (isDynamicOffset2)
                {
                    if (localFunctionTable == null)
                    {
                        localFunctionTable = instanceConstructorIL.DeclareLocal(typeof(FunctionTable));
                        instanceConstructorIL.EmitLoadArg(0);
                        instanceConstructorIL.Emit(OpCodes.Ldfld, functionTable!);
                        instanceConstructorIL.Emit(OpCodes.Stloc, localFunctionTable);
                    }
                    
                    dynamicOffsetLocal2 = instanceConstructorIL.DeclareLocal(typeof(uint));
                    var global2 = (context.Globals ?? throw new CompilerException("global.get requires a global section."))[globalIndex2];
                    if (global2.RequiresInstance)
                        instanceConstructorIL.EmitLoadArg(0);
                    instanceConstructorIL.Emit(OpCodes.Call, global2.Getter);
                    instanceConstructorIL.Emit(OpCodes.Stloc, dynamicOffsetLocal2);
                }

                activeSegments.Add((tableIdx2, constOffset2, isDynamicOffset2, globalIndex2, funcIndicesArr2, dynamicOffsetLocal2));
                continue;
            }

            // Kind 3: declarative, func indices.
            if (kind == 3)
            {
                context.ElementSegments[(uint)i] = exportsBuilder.DefineField(
                    $"☣ DeclarativeElem {i}",
                    typeof(Delegate[]),
                    FieldAttributes.Private);
                context.ElementSegmentTypes[(uint)i] = ElementType.FunctionReference;
                reader.ReadByte(); // elemkind (0x00 = funcref)
                var elements3 = reader.ReadVarUInt32();
                for (var j = 0u; j < elements3; j++)
                    context.DeclaredFunctionReferences.Add(reader.ReadVarUInt32());
                continue;
            }

            // Kind 7: declarative, init-exprs.
            if (kind == 7)
            {
                var reftype7 = (ElementType)reader.ReadVarInt7();
                context.ElementSegments[(uint)i] = exportsBuilder.DefineField(
                    $"☣ DeclarativeElem {i}",
                    typeof(Delegate[]),
                    FieldAttributes.Private);
                context.ElementSegmentTypes[(uint)i] = reftype7;
                var elemCount7 = reader.ReadVarUInt32();
                for (var j = 0u; j < elemCount7; j++)
                {
                    var preExprOffset = reader.Offset;
                    var expr = Instruction.ParseInitializerExpression(reader).ToArray();
                    if (expr.Length != 2)
                        throw new ModuleLoadException("type mismatch", preExprOffset);
                    if (expr[0] is Instructions.RefFunc rf7)
                    {
                        if (reftype7 != ElementType.FunctionReference)
                            throw new ModuleLoadException("type mismatch", preExprOffset);
                        if (functionSignatures == null || rf7.Index >= (uint)functionSignatures.Length)
                            throw new ModuleLoadException($"Element segment {i}: function index {rf7.Index} is unknown.", preExprOffset);
                        context.DeclaredFunctionReferences.Add(rf7.Index);
                    }
                    else if (expr[0] is Instructions.RefNull rn7)
                    {
                        var exprElemType = rn7.Type == WebAssemblyValueType.FuncRef ? ElementType.FunctionReference : ElementType.ExternRef;
                        if (exprElemType != reftype7)
                            throw new ModuleLoadException("type mismatch", preExprOffset);
                    }
                    else
                        throw new ModuleLoadException("type mismatch", preExprOffset);
                }
                continue;
            }

            if (kind != 0)
            {
                SkipElementSegment(reader, kind);
                continue;
            }

            // Kind 0: active, table 0, i32 constant offset, func indices.
            // Register in ElementSegments as null (active segments are dropped after instantiation).
            context.ElementSegments[(uint)i] = exportsBuilder.DefineField(
                $"☣ ActiveElem {i}",
                typeof(Delegate[]),
                FieldAttributes.Private);
            context.ElementSegmentTypes[(uint)i] = ElementType.FunctionReference;

            uint? constOffset = null;
            uint globalIndex = 0;
            bool isDynamicOffset = false;
            {
                var preInitializerOffset = reader.Offset;
                var initializer = Instruction.ParseInitializerExpression(reader).ToArray();
                if (initializer.Length == 2 && initializer[0] is Instructions.Int32Constant ic && initializer[1] is Instructions.End)
                {
                    constOffset = (uint)ic.Value;
                }
                else if (initializer.Length == 2 && initializer[0] is Instructions.GlobalGet gg && initializer[1] is Instructions.End)
                {
                    if (gg.Index >= (uint)context.ImportedGlobalCount)
                        throw new ModuleLoadException("unknown global", preInitializerOffset);
                    if (context.Globals != null && gg.Index < (uint)context.Globals.Length && context.Globals[gg.Index].Setter != null)
                        throw new ModuleLoadException("constant expression required", preInitializerOffset);
                    globalIndex = gg.Index;
                    isDynamicOffset = true;
                }
                else
                {
                    throw new ModuleLoadException("Initializer expression support for the Element section is limited to a single Int32 constant or global.get followed by end.", preInitializerOffset);
                }
            }

            var preElementOffset = reader.Offset;
            var elements = reader.ReadVarUInt32();

            if (functionTable == null)
                throw new ModuleLoadException("Active element segment requires a table section or import.", preElementOffset);

            if (localFunctionTable == null)
            {
                localFunctionTable = instanceConstructorIL.DeclareLocal(typeof(FunctionTable));
                instanceConstructorIL.EmitLoadArg(0);
                instanceConstructorIL.Emit(OpCodes.Ldfld, functionTable);
                instanceConstructorIL.Emit(OpCodes.Stloc, localFunctionTable);
            }

            // For dynamic offsets (global.get), emit IL to load the global value into a local.
            LocalBuilder? dynamicOffsetLocal = null;
            if (isDynamicOffset)
            {
                dynamicOffsetLocal = instanceConstructorIL.DeclareLocal(typeof(uint));
                var global = (context.Globals ?? throw new CompilerException("global.get requires a global section."))[globalIndex];
                if (global.RequiresInstance)
                    instanceConstructorIL.EmitLoadArg(0);
                instanceConstructorIL.Emit(OpCodes.Call, global.Getter);
                instanceConstructorIL.Emit(OpCodes.Stloc, dynamicOffsetLocal);
            }

            var funcIndicesArr = new uint[elements];
            if (elements > 0)
            {
                if (functionSignatures == null || internalFunctions == null)
                    throw new ModuleLoadException("Element section must be empty if there are no functions to reference.", preElementOffset);
                for (var j = 0u; j < elements; j++)
                {
                    var funcIdx = reader.ReadVarUInt32();
                    funcIndicesArr[j] = funcIdx;
                    context.DeclaredFunctionReferences.Add(funcIdx);
                }
            }

            activeSegments.Add((0, constOffset, isDynamicOffset, globalIndex, funcIndicesArr, dynamicOffsetLocal));
        }

        // Create locals for all tables referenced by active segments.
        var tableLocals = new Dictionary<uint, LocalBuilder>();
        foreach (var (tableIndex, _, _, _, _, _) in activeSegments)
        {
            if (!tableLocals.ContainsKey(tableIndex))
            {
                if (tableIndex >= (uint)context.Tables.Count)
                    throw new ModuleLoadException($"Element segment references table {tableIndex} but only {context.Tables.Count} tables exist.", 0);
                
                var tableField = context.Tables[(int)tableIndex];
                if (tableField == null)
                    throw new ModuleLoadException($"Element segment references table {tableIndex} which is null.", 0);
                    
                var tableLocal = instanceConstructorIL.DeclareLocal(typeof(FunctionTable));
                instanceConstructorIL.EmitLoadArg(0);
                instanceConstructorIL.Emit(OpCodes.Ldfld, tableField);
                instanceConstructorIL.Emit(OpCodes.Stloc, tableLocal);
                tableLocals[tableIndex] = tableLocal;
            }
        }

        foreach (var (tableIndex, _, _, _, _, _) in activeGlobalRefSegments)
        {
            if (!tableLocals.ContainsKey(tableIndex))
            {
                if (tableIndex >= (uint)context.Tables.Count)
                    throw new ModuleLoadException($"Element segment references table {tableIndex} but only {context.Tables.Count} tables exist.", 0);

                var tableField = context.Tables[(int)tableIndex];
                if (tableField == null)
                    throw new ModuleLoadException($"Element segment references table {tableIndex} which is null.", 0);

                var tableLocal = instanceConstructorIL.DeclareLocal(typeof(FunctionTable));
                instanceConstructorIL.EmitLoadArg(0);
                instanceConstructorIL.Emit(OpCodes.Ldfld, tableField);
                instanceConstructorIL.Emit(OpCodes.Stloc, tableLocal);
                tableLocals[tableIndex] = tableLocal;
            }
        }

        var externRefTableLocals = new Dictionary<uint, LocalBuilder>();
        foreach (var (tableIndex, _, _, _, _, _) in activeExternRefSegments)
        {
            if (!externRefTableLocals.ContainsKey(tableIndex))
            {
                if (tableIndex >= (uint)context.Tables.Count)
                    throw new ModuleLoadException($"Element segment references table {tableIndex} but only {context.Tables.Count} tables exist.", 0);

                var tableField = context.Tables[(int)tableIndex];
                if (tableField == null)
                    throw new ModuleLoadException($"Element segment references table {tableIndex} which is null.", 0);

                var tableLocal = instanceConstructorIL.DeclareLocal(typeof(ExternRefTable));
                instanceConstructorIL.EmitLoadArg(0);
                instanceConstructorIL.Emit(OpCodes.Ldfld, tableField);
                instanceConstructorIL.Emit(OpCodes.Stloc, tableLocal);
                externRefTableLocals[tableIndex] = tableLocal;
            }
        }

        // Pass 1: emit all bounds checks before any writes (atomicity guarantee).
        foreach (var (tableIndex, constOffset, isDynamic, globalIndex, funcIndices, dynLocal) in activeSegments)
        {
            var elements = (uint)funcIndices.Length;
            var isBigEnough = instanceConstructorIL.DefineLabel();
            var tableLocal = tableLocals[tableIndex];
            instanceConstructorIL.Emit(OpCodes.Ldloc, tableLocal);
            instanceConstructorIL.Emit(OpCodes.Call, FunctionTable.LengthGetter);
            if (isDynamic)
            {
                instanceConstructorIL.Emit(OpCodes.Ldloc, dynLocal!);
                if (elements > 0)
                {
                    instanceConstructorIL.EmitLoadConstant((int)elements);
                    instanceConstructorIL.Emit(OpCodes.Add_Ovf_Un);
                }
            }
            else
            {
                instanceConstructorIL.EmitLoadConstant(checked(constOffset!.Value + elements));
            }
            instanceConstructorIL.Emit(OpCodes.Bge_Un, isBigEnough);
            instanceConstructorIL.Emit(OpCodes.Newobj, typeof(OverflowException).GetConstructor(Type.EmptyTypes)!);
            instanceConstructorIL.Emit(OpCodes.Throw);
            instanceConstructorIL.MarkLabel(isBigEnough);
        }

        foreach (var (tableIndex, constOffset, isDynamic, globalIndex, entries, dynLocal) in activeGlobalRefSegments)
        {
            var elements = (uint)entries.Length;
            var isBigEnough = instanceConstructorIL.DefineLabel();
            var tableLocal = tableLocals[tableIndex];
            instanceConstructorIL.Emit(OpCodes.Ldloc, tableLocal);
            instanceConstructorIL.Emit(OpCodes.Call, FunctionTable.LengthGetter);
            if (isDynamic)
            {
                instanceConstructorIL.Emit(OpCodes.Ldloc, dynLocal!);
                if (elements > 0)
                {
                    instanceConstructorIL.EmitLoadConstant((int)elements);
                    instanceConstructorIL.Emit(OpCodes.Add_Ovf_Un);
                }
            }
            else
            {
                instanceConstructorIL.EmitLoadConstant(checked(constOffset!.Value + elements));
            }
            instanceConstructorIL.Emit(OpCodes.Bge_Un, isBigEnough);
            instanceConstructorIL.Emit(OpCodes.Newobj, typeof(OverflowException).GetConstructor(Type.EmptyTypes)!);
            instanceConstructorIL.Emit(OpCodes.Throw);
            instanceConstructorIL.MarkLabel(isBigEnough);
        }

        foreach (var (tableIndex, constOffset, isDynamic, globalIndex, length, dynLocal) in activeExternRefSegments)
        {
            var isBigEnough = instanceConstructorIL.DefineLabel();
            var tableLocal = externRefTableLocals[tableIndex];
            instanceConstructorIL.Emit(OpCodes.Ldloc, tableLocal);
            instanceConstructorIL.Emit(OpCodes.Call, ExternRefTable.LengthGetter);
            if (isDynamic)
            {
                instanceConstructorIL.Emit(OpCodes.Ldloc, dynLocal!);
                if (length > 0)
                {
                    instanceConstructorIL.EmitLoadConstant((int)length);
                    instanceConstructorIL.Emit(OpCodes.Add_Ovf_Un);
                }
            }
            else
            {
                instanceConstructorIL.EmitLoadConstant(checked(constOffset!.Value + length));
            }
            instanceConstructorIL.Emit(OpCodes.Bge_Un, isBigEnough);
            instanceConstructorIL.Emit(OpCodes.Newobj, typeof(OverflowException).GetConstructor(Type.EmptyTypes)!);
            instanceConstructorIL.Emit(OpCodes.Throw);
            instanceConstructorIL.MarkLabel(isBigEnough);
        }

        if (activeSegments.Count == 0 && activeGlobalRefSegments.Count == 0 && activeExternRefSegments.Count == 0 && passiveSegments.Count == 0)
            return null;

        // Return a deferred action that emits element writes. The caller must invoke this AFTER
        // emitting all other section checks (data segment bounds) so that if any check fails,
        // no writes to a shared imported table have already occurred.
        var capturedSegments = activeSegments;
        var capturedGlobalRefSegments = activeGlobalRefSegments;
        var capturedExternRefSegments = activeExternRefSegments;
        var capturedPassiveSegments = passiveSegments;
        var capturedTableLocals = tableLocals;
        var capturedExternRefTableLocals = externRefTableLocals;
        var capturedSetter = setter;
        return () =>
        {
            foreach (var (tableIndex, constOffset, isDynamic, _, funcIndices, dynLocal) in capturedSegments)
            {
                var elements = (uint)funcIndices.Length;
                if (elements == 0)
                    continue;

                var tableLocal = capturedTableLocals[tableIndex];

                for (var j = 0u; j < elements; j++)
                {
                    var functionIndex = funcIndices[j];
                    if (functionIndex == uint.MaxValue)
                    {
                        // ref.null slot: store null delegate (skip writing — FunctionTable default is null).
                        continue;
                    }
                    instanceConstructorIL.Emit(OpCodes.Ldloc, tableLocal);
                    if (isDynamic)
                    {
                        instanceConstructorIL.Emit(OpCodes.Ldloc, dynLocal!);
                        if (j > 0)
                        {
                            instanceConstructorIL.EmitLoadConstant((int)j);
                            instanceConstructorIL.Emit(OpCodes.Add);
                        }
                    }
                    else
                    {
                        instanceConstructorIL.EmitLoadConstant(constOffset!.Value + j);
                    }

                    instanceConstructorIL.EmitLoadArg(0);
                    instanceConstructorIL.Emit(OpCodes.Ldfld, context.FunctionReferences!);
                    instanceConstructorIL.EmitLoadConstant((int)functionIndex);
                    instanceConstructorIL.Emit(OpCodes.Ldelem_Ref);

                    instanceConstructorIL.Emit(OpCodes.Call, capturedSetter);
                }
            }

            foreach (var (tableIndex, constOffset, isDynamic, _, entries, dynLocal) in capturedGlobalRefSegments)
            {
                if (entries.Length == 0)
                    continue;

                var tableLocal = capturedTableLocals[tableIndex];

                for (var j = 0u; j < entries.Length; j++)
                {
                    instanceConstructorIL.Emit(OpCodes.Ldloc, tableLocal);
                    if (isDynamic)
                    {
                        instanceConstructorIL.Emit(OpCodes.Ldloc, dynLocal!);
                        if (j > 0)
                        {
                            instanceConstructorIL.EmitLoadConstant((int)j);
                            instanceConstructorIL.Emit(OpCodes.Add);
                        }
                    }
                    else
                    {
                        instanceConstructorIL.EmitLoadConstant(constOffset!.Value + j);
                    }

                    var entry = entries[j];
                    switch (entry.Kind)
                    {
                        case 0:
                            instanceConstructorIL.Emit(OpCodes.Ldnull);
                            break;
                        case 1:
                            instanceConstructorIL.EmitLoadArg(0);
                            instanceConstructorIL.Emit(OpCodes.Ldfld, context.FunctionReferences!);
                            instanceConstructorIL.EmitLoadConstant((int)entry.Index);
                            instanceConstructorIL.Emit(OpCodes.Ldelem_Ref);
                            break;
                        case 2:
                            var global = (context.Globals ?? throw new CompilerException("global.get requires a global section."))[entry.Index];
                            if (global.RequiresInstance)
                                instanceConstructorIL.EmitLoadArg(0);
                            instanceConstructorIL.Emit(OpCodes.Call, global.Getter);
                            break;
                        default:
                            throw new CompilerException($"Unknown active global-ref segment entry kind {entry.Kind}.");
                    }

                    instanceConstructorIL.Emit(OpCodes.Call, capturedSetter);
                }
            }

            foreach (var (tableIndex, constOffset, isDynamic, _, length, dynLocal) in capturedExternRefSegments)
            {
                if (length == 0)
                    continue;

                var tableLocal = capturedExternRefTableLocals[tableIndex];

                for (var j = 0u; j < length; j++)
                {
                    instanceConstructorIL.Emit(OpCodes.Ldloc, tableLocal);
                    if (isDynamic)
                    {
                        instanceConstructorIL.Emit(OpCodes.Ldloc, dynLocal!);
                        if (j > 0)
                        {
                            instanceConstructorIL.EmitLoadConstant((int)j);
                            instanceConstructorIL.Emit(OpCodes.Add);
                        }
                    }
                    else
                    {
                        instanceConstructorIL.EmitLoadConstant(constOffset!.Value + j);
                    }

                    instanceConstructorIL.Emit(OpCodes.Ldnull);
                    instanceConstructorIL.Emit(OpCodes.Call, ExternRefTable.IndexSetter);
                }
            }
            
            // Emit passive element segments (deferred until after Code section so internalFunctions[] is populated).
            foreach (var (segIndex, segField, funcIndices, elemType) in capturedPassiveSegments)
            {
                var arrLocal = instanceConstructorIL.DeclareLocal(typeof(Delegate[]));
                instanceConstructorIL.EmitLoadConstant((int)funcIndices.Length);
                instanceConstructorIL.Emit(OpCodes.Newarr, typeof(Delegate));
                instanceConstructorIL.Emit(OpCodes.Stloc, arrLocal);

                for (var j = 0u; j < funcIndices.Length; j++)
                {
                    var functionIndex = funcIndices[j];
                    if (functionIndex == uint.MaxValue)
                        continue; // null slot — leave array element as null

                    instanceConstructorIL.Emit(OpCodes.Ldloc, arrLocal);
                    instanceConstructorIL.EmitLoadConstant((int)j);
                    instanceConstructorIL.EmitLoadArg(0);
                    instanceConstructorIL.Emit(OpCodes.Ldfld, context.FunctionReferences!);
                    instanceConstructorIL.EmitLoadConstant((int)functionIndex);
                    instanceConstructorIL.Emit(OpCodes.Ldelem_Ref);
                    instanceConstructorIL.Emit(OpCodes.Stelem_Ref);
                }

                instanceConstructorIL.EmitLoadArg(0);
                instanceConstructorIL.Emit(OpCodes.Ldloc, arrLocal);
                instanceConstructorIL.Emit(OpCodes.Stfld, segField);
            }
        };
    }

    static void SkipElementSegment(Reader reader, uint kind)
    {
        // Parse and discard a non-kind-0 element segment to advance the reader past it.
        switch (kind)
        {
            case 1: // passive, func indices, preceded by 0x00 elemkind
            case 3: // declarative, func indices, preceded by 0x00 elemkind
                reader.ReadByte(); // elemkind
                SkipVarUInt32Vector(reader);
                break;

            case 2: // active explicit table, func indices
                reader.ReadVarUInt32(); // table index
                SkipInitializerExpression(reader);
                reader.ReadByte(); // elemkind
                SkipVarUInt32Vector(reader);
                break;

            case 4: // active table 0, init exprs
                SkipInitializerExpression(reader);
                SkipInitExprVector(reader);
                break;

            case 5: // passive, init exprs, reftype
            case 7: // declarative, init exprs, reftype
                reader.ReadVarInt7(); // reftype
                SkipInitExprVector(reader);
                break;

            case 6: // active explicit table, init exprs
                reader.ReadVarUInt32(); // table index
                SkipInitializerExpression(reader);
                reader.ReadVarInt7(); // reftype
                SkipInitExprVector(reader);
                break;

            default:
                throw new ModuleLoadException($"Unsupported element segment kind {kind}.", reader.Offset);
        }
    }

    static void SkipVarUInt32Vector(Reader reader)
    {
        var n = reader.ReadVarUInt32();
        for (var i = 0u; i < n; i++)
            reader.ReadVarUInt32();
    }

    static void SkipInitializerExpression(Reader reader)
    {
        foreach (var _ in Instruction.ParseInitializerExpression(reader)) { }
    }

    static void SkipInitExprVector(Reader reader)
    {
        var n = reader.ReadVarUInt32();
        for (var i = 0u; i < n; i++)
            SkipInitializerExpression(reader);
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

            il.Emit(OpCodes.Call, typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.EnsureSufficientExecutionStack))!);

            foreach (var instruction in Instruction.Parse(reader))
            {
                instruction.Compile(context);
                context.Previous = instruction.OpCode;
            }

            if (reader.Offset - startingOffset != byteLength)
                throw new ModuleLoadException($"Instruction sequence reader ended after reading {reader.Offset - startingOffset} characters, expected {byteLength}.", reader.Offset);
        }
    }

    static void SectionData(Reader reader, CompilationContext context, FieldBuilder? memory, ILGenerator instanceConstructorIL, TypeBuilder exportsBuilder, long sectionOffset = 0)
    {
        var count = reader.ReadVarUInt32();

        context.Reset(
            instanceConstructorIL,
            Signature.Empty,
            Signature.Empty.RawParameterTypes
            );
        var block = new Instructions.Block(BlockType.Int32);

        // Buffer active segment write info: (address local, initialized-data field, data length).
        // All bounds checks are emitted first; writes are deferred until all checks pass.
        var pendingWrites = new System.Collections.Generic.List<(LocalBuilder AddrLocal, FieldBuilder DataField, int DataLen)>();

        for (var i = 0u; i < count; i++)
        {
            var startingOffset = reader.Offset;
            var kind = reader.ReadVarUInt32();

            if (kind == 1)
            {
                // Passive segment: initialize the pre-allocated byte[] instance field (registered during DataCount).
                var rawBytes = reader.ReadBytes(reader.ReadVarUInt32());

                // Get the pre-allocated field (registered during DataCount).
                if (!context.DataSegments.TryGetValue(i, out var segField))
                {
                    // No DataCount section — allocate now (fallback for modules without DataCount).
                    segField = exportsBuilder.DefineField(
                        $"☣ PassiveData {i}",
                        typeof(byte[]),
                        FieldAttributes.Private);
                    context.DataSegments[i] = segField;
                }

                if (rawBytes.Length > 0)
                {
                    if (rawBytes.Length > 0x3f0000)
                        throw new NotSupportedException($"Passive data segment {i} is length {rawBytes.Length}, exceeding the implementation limit.");

                    // Use the same RVA-field + RuntimeHelpers.InitializeArray pattern that C# uses for
                    // array initializers: create new byte[], then bulk-initialize it from the PE's .sdata.
                    var initField = exportsBuilder.DefineInitializedData(
                        $"☣ PassiveDataInit {i}", rawBytes,
                        FieldAttributes.Assembly | FieldAttributes.InitOnly);
                    var dupLocal = instanceConstructorIL.DeclareLocal(typeof(byte[]));

                    instanceConstructorIL.Emit(OpCodes.Ldc_I4, rawBytes.Length);
                    instanceConstructorIL.Emit(OpCodes.Newarr, typeof(byte));
                    instanceConstructorIL.Emit(OpCodes.Dup);
                    instanceConstructorIL.Emit(OpCodes.Ldtoken, initField);
                    instanceConstructorIL.Emit(OpCodes.Call,
                        typeof(System.Runtime.CompilerServices.RuntimeHelpers).GetMethod(
                            nameof(System.Runtime.CompilerServices.RuntimeHelpers.InitializeArray),
                            [typeof(Array), typeof(RuntimeFieldHandle)])!);
                    instanceConstructorIL.Emit(OpCodes.Stloc, dupLocal);

                    // this.segField = newArray
                    instanceConstructorIL.Emit(OpCodes.Ldarg_0);
                    instanceConstructorIL.Emit(OpCodes.Ldloc, dupLocal);
                    instanceConstructorIL.Emit(OpCodes.Stfld, segField);
                }
                continue;
            }

            // Kind 2: active with explicit memory index — read and ignore the memory index (treat same as kind 0).
            if (kind == 2)
                reader.ReadVarUInt32(); // memory index (must be 0 for now)
            else if (kind != 0)
                throw new ModuleLoadException($"Data segment kind must be 0, 1, or 2, found {kind}.", startingOffset);

            // Active data segments (kind 0 or 2) require a memory.
            if (memory == null)
                throw new ModuleLoadException("Active data segment cannot be used unless a memory section is defined.", startingOffset);

            // Each active segment gets its own address local so writes can be deferred.
            var segAddress = instanceConstructorIL.DeclareLocal(typeof(uint));

            block.Compile(context); //Prevents "end" instruction of the initializer expression from becoming a return.
            foreach (var instruction in Instruction.ParseInitializerExpression(reader))
            {
                if (instruction is Instructions.GlobalGet ggData)
                {
                    if (ggData.Index >= (uint)context.ImportedGlobalCount)
                        throw new ModuleLoadException("unknown global", reader.Offset);
                    if (context.Globals != null && ggData.Index < (uint)context.Globals.Length && context.Globals[ggData.Index].Setter != null)
                        throw new ModuleLoadException("constant expression required", reader.Offset);
                }
                instruction.Compile(context);
                context.Previous = instruction.OpCode;
            }
            context.Stack.Pop();
            context.BlockContexts.Remove(context.Depth.Count);
            instanceConstructorIL.Emit(OpCodes.Stloc, segAddress);

            var data = reader.ReadBytes(reader.ReadVarUInt32());

            // Spec: dst + len <= memory.size must hold even for zero-length segments.
            // RangeCheck8 checks: memory.size > address + 1 (i.e. address + 1 byte is in range).
            // For len == 0: check address + 0 <= memory.size, equivalent to RangeCheck8(address - 1).
            // For len > 0:  check address + len - 1 is in range, equivalent to RangeCheck8(address + len - 1).
            // Special case: address == 0 && len == 0 is always valid — skip check entirely.
            if (data.Length == 0)
            {
                // Emit a runtime check: if address > 0, verify address - 1 is in range.
                var skipCheck = instanceConstructorIL.DefineLabel();
                instanceConstructorIL.Emit(OpCodes.Ldloc, segAddress);
                instanceConstructorIL.Emit(OpCodes.Brfalse_S, skipCheck);

                instanceConstructorIL.Emit(OpCodes.Ldloc, segAddress);
                instanceConstructorIL.Emit(OpCodes.Ldc_I4_1);
                instanceConstructorIL.Emit(OpCodes.Sub_Ovf_Un);
                instanceConstructorIL.Emit(OpCodes.Ldarg_0);
                instanceConstructorIL.Emit(OpCodes.Call, context[HelperMethod.RangeCheck8, Instructions.MemoryImmediateInstruction.CreateRangeCheck]);
                instanceConstructorIL.Emit(OpCodes.Pop);

                instanceConstructorIL.MarkLabel(skipCheck);
                // No write needed for zero-length segment.
                continue;
            }

            //Ensure sufficient memory is allocated, error if max is exceeded.
            // Check the last byte (address + data.Length - 1) is within bounds. RangeCheck8 checks ptr+1 <= size.
            instanceConstructorIL.Emit(OpCodes.Ldloc, segAddress);
            instanceConstructorIL.Emit(OpCodes.Ldc_I4, data.Length - 1);
            instanceConstructorIL.Emit(OpCodes.Add_Ovf_Un);

            instanceConstructorIL.Emit(OpCodes.Ldarg_0);

            instanceConstructorIL.Emit(OpCodes.Call, context[HelperMethod.RangeCheck8, Instructions.MemoryImmediateInstruction.CreateRangeCheck]);
            instanceConstructorIL.Emit(OpCodes.Pop);

            if (data.Length > 0x3f0000) //Limitation of DefineInitializedData, can be corrected by splitting the data.
                throw new NotSupportedException($"Data segment {i} is length {data.Length}, exceeding the current implementation limit of 4128768.");

            var dataField = exportsBuilder.DefineInitializedData($"☣ Data {i}", data, FieldAttributes.Assembly | FieldAttributes.InitOnly);
            pendingWrites.Add((segAddress, dataField, data.Length));
        }

        // Emit all writes after all bounds checks have passed (atomicity guarantee).
        foreach (var (addrLocal, dataField, dataLen) in pendingWrites)
        {
            instanceConstructorIL.Emit(OpCodes.Ldarg_0);
            instanceConstructorIL.Emit(OpCodes.Ldfld, memory!);
            instanceConstructorIL.Emit(OpCodes.Call, UnmanagedMemory.StartGetter);
            instanceConstructorIL.Emit(OpCodes.Ldloc, addrLocal);
            instanceConstructorIL.Emit(OpCodes.Conv_I);
            instanceConstructorIL.Emit(OpCodes.Add_Ovf_Un);

            instanceConstructorIL.Emit(OpCodes.Ldsflda, dataField);

            instanceConstructorIL.Emit(OpCodes.Ldc_I4, dataLen);

            instanceConstructorIL.Emit(OpCodes.Cpblk);
        }
    }

    static FieldBuilder CreateFunctionTableField(TypeBuilder exportsBuilder, CompilerConfiguration configuration)
        => exportsBuilder.DefineField(
            "☣ FunctionTable",
            configuration.NeutralizeType(typeof(FunctionTable)),
            FieldAttributes.Private | FieldAttributes.InitOnly);

    static void EmitFunctionReferencesInitialization(
        ILGenerator il,
        FieldBuilder functionReferencesField,
        MethodInfo[]? internalFunctions,
        Signature[] functionSignatures,
        int importedFunctions,
        CompilerConfiguration configuration,
        TypeBuilder exportsBuilder)
    {
        var totalFunctions = internalFunctions?.Length ?? 0;
        
        // Create the delegate array in a local variable
        var arrLocal = il.DeclareLocal(typeof(Delegate[]));
        il.EmitLoadConstant(totalFunctions);
        il.Emit(OpCodes.Newarr, typeof(Delegate));
        il.Emit(OpCodes.Stloc, arrLocal);
        
        // Populate function delegates
        // internalFunctions contains ALL functions (imported + internal):
        //   [0..importedFunctions-1]: MethodInfo for invoker wrappers of imported functions
        //   [importedFunctions..total-1]: MethodInfo for internal functions (may be null if not yet compiled)
        // We create delegates for ALL non-null entries.
        if (internalFunctions != null && functionSignatures != null)
        {
            for (var i = 0; i < internalFunctions.Length && i < functionSignatures.Length; i++)
            {
                var method = internalFunctions[i];
                if (method == null)
                    continue; // Skip not-yet-compiled functions
                    
                var signature = functionSignatures[i];
                
                // Get the appropriate delegate type for this function's signature
                var parms = signature.ParameterTypes;
                var returns = signature.ReturnTypes;
                var clrRetCount = returns.Length > 1 ? 1 : returns.Length;
                var delegateType = configuration.GetDelegateForType(parms.Length, clrRetCount);
                
                if (delegateType != null)
                {
                    if (delegateType.IsGenericType)
                    {
                        var typeArgs = Compilation.MultiValueHelper.DelegateTypeArgs(parms, returns);
                        delegateType = delegateType.MakeGenericType(typeArgs);
                    }
                    
                    var delegateCtor = delegateType.GetTypeInfo().DeclaredConstructors.FirstOrDefault();
                    if (delegateCtor == null)
                    {
                        // Skip this function if we can't find the delegate constructor
                        continue;
                    }

                    var wrapper = exportsBuilder.DefineMethod(
                        $"↪ {i}",
                        MethodAttributes.Private | MethodAttributes.HideBySig,
                        Compilation.MultiValueHelper.ClrReturnType(signature.ReturnTypes),
                        signature.ParameterTypes);
                    var wrapperIL = wrapper.GetILGenerator();
                    for (var parameterIndex = 0; parameterIndex < signature.ParameterTypes.Length; parameterIndex++)
                        wrapperIL.EmitLoadArg(parameterIndex + 1);
                    wrapperIL.EmitLoadArg(0);
                    wrapperIL.Emit(OpCodes.Call, method);
                    wrapperIL.Emit(OpCodes.Ret);
                    
                    // Store delegate in array: array[i] = new DelegateType(this, wrapper)
                    il.Emit(OpCodes.Ldloc, arrLocal);
                    il.EmitLoadConstant(i);
                    il.EmitLoadArg(0);
                    il.Emit(OpCodes.Ldftn, wrapper);
                    il.Emit(OpCodes.Newobj, delegateCtor);
                    il.Emit(OpCodes.Stelem_Ref);
                }
            }
        }
        
        // Store array in field: this.functionRefs = array
        il.EmitLoadArg(0);
        il.Emit(OpCodes.Ldloc, arrLocal);
        il.Emit(OpCodes.Stfld, functionReferencesField);
    }

    static FieldBuilder CreateMemoryField(TypeBuilder exportsBuilder, CompilerConfiguration configuration)
        => exportsBuilder.DefineField(
            "☣ Memory",
            configuration.NeutralizeType(typeof(UnmanagedMemory)),
            PrivateReadonlyField);
}
