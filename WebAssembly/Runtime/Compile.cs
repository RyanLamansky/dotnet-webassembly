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
                    ArgumentNullException.ThrowIfNull(module, nameof(module));
                    ArgumentNullException.ThrowIfNull(field, nameof(field));

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
        public readonly ValType Type = ReadType(reader);

        // Locals can be typed references in WebAssembly 3.0, so read the full reference type (not just a varint7).
        private static ValType ReadType(Reader reader)
        {
            var value = ReferenceType.ReadValueType(reader, out var refType);
            return ValType.FromValueType(value, refType);
        }
    }

    // A concrete-index typed reference must point at a type the module actually defines; otherwise it is the
    // spec's "unknown type" validation failure.
    static void ValidateTypeIndex(ValType valType, int typeCount, long offset)
    {
        if (valType.IsReference && valType.IsIndexed && valType.TypeIndex >= (uint)typeCount)
            throw new ModuleLoadException($"unknown type {valType.TypeIndex}: the module defines only {typeCount} type(s).", offset);
    }

    // Interprets an already-consumed table element-type lead byte, which in WebAssembly 3.0 is any reference type
    // (funcref/externref shorthands or a long-form typed reference). Rejects a non-reference type.
    static ValType ReadTableElementTypeFromLead(Reader reader, sbyte lead, long offset)
    {
        var value = (WebAssemblyValueType)lead;
        ReferenceType? refType = null;
        if (value is WebAssemblyValueType.Reference or WebAssemblyValueType.NullableReference)
            refType = ReferenceType.Parse(reader, lead);
        var valType = ValType.FromValueType(value, refType);
        if (!valType.IsReference)
            throw new ModuleLoadException($"Table element type {value} is not a reference type.", offset);
        return valType;
    }

    // Defines a table backing field, emits its construction into the instance constructor, and — for a WebAssembly
    // 3.0 table-with-initializer — fills every slot in [0, minimum) with the evaluated constant expression.
    static void EmitTableDefinition(
        CompilationContext context,
        TypeBuilder exportsBuilder,
        ILGenerator il,
        CompilerConfiguration configuration,
        ref FieldBuilder? functionTable,
        ValType elementValType,
        ResizableLimits limits,
        IList<Instruction>? tableInit,
        long offset,
        Action ensureFunctionReferences)
    {
        // A non-nullable element type with no initializer cannot be default-filled (there is no null to seed the
        // slots) — the spec rejects it as a "type mismatch".
        if (!elementValType.IsNullable && tableInit == null)
            throw new ModuleLoadException("type mismatch: a table with a non-nullable element type requires an initializer expression.", offset);

        var isFuncTable = CompilationContext.IsFunctionReference(elementValType);
        var tableType = configuration.NeutralizeType(isFuncTable ? typeof(FunctionTable) : typeof(ExternRefTable));
        var tableField = exportsBuilder.DefineField($"☣ Table{context.Tables.Count}", tableType, FieldAttributes.Private | FieldAttributes.InitOnly);

        context.Tables.Add(tableField);
        context.TableElementTypes.Add(elementValType);
        context.TableIs64.Add(limits.Is64Bit);

        // Retain the first funcref table for the WASM 1.0 single-table paths (call_indirect, active elements).
        if (functionTable == null && isFuncTable)
            functionTable = tableField;

        il.EmitLoadArg(0);
        il.EmitLoadConstant(limits.ClampedMinimum);
        if (limits.Maximum64.HasValue)
        {
            il.EmitLoadConstant(limits.ClampedMaximum);
            il.Emit(OpCodes.Newobj, configuration.NeutralizeType(typeof(uint?))
                .GetTypeInfo()
                .DeclaredConstructors
                .Where(constructor =>
                {
                    var parms = constructor.GetParameters();
                    return parms.Length == 1 && parms[0].ParameterType == configuration.NeutralizeType(typeof(uint));
                })
                .Single());
            il.Emit(OpCodes.Newobj, tableType
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
            il.Emit(OpCodes.Newobj, tableType
                .GetTypeInfo()
                .DeclaredConstructors
                .Where(constructor =>
                {
                    var parms = constructor.GetParameters();
                    return parms.Length == 1 && parms[0].ParameterType == configuration.NeutralizeType(typeof(uint));
                })
                .Single());
        }

        // Record the index type on the table so an importer of this (exported) table can reject an i32/i64
        // mismatch. Emitted only for table64, leaving 32-bit modules unchanged.
        EmitSetIs64(il, configuration, typeof(TableImport), limits.Is64Bit);
        il.Emit(OpCodes.Stfld, tableField);

        if (tableInit != null)
        {
            // The initializer must produce a reference that is a subtype of the table's element type.
            ValidateElementInitExpr(tableInit, elementValType, context);

            // Fill every slot [0, minimum) with the value produced by the initializer constant expression.
            if (tableInit.Any(inst => inst is Instructions.RefFunc))
                ensureFunctionReferences();

            var valLocal = il.DeclareLocal(isFuncTable ? typeof(Delegate) : typeof(object));
            EmitElementRefValue(il, context, tableInit);
            il.Emit(OpCodes.Stloc, valLocal);

            var setter = isFuncTable ? FunctionTable.IndexSetter : ExternRefTable.IndexSetter;
            var iLocal = il.DeclareLocal(typeof(int));
            var loopStart = il.DefineLabel();
            var loopEnd = il.DefineLabel();
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Stloc, iLocal);
            il.MarkLabel(loopStart);
            il.Emit(OpCodes.Ldloc, iLocal);
            il.EmitLoadConstant(limits.ClampedMinimum);
            il.Emit(OpCodes.Bge_Un, loopEnd);
            il.EmitLoadArg(0);
            il.Emit(OpCodes.Ldfld, tableField);
            il.Emit(OpCodes.Ldloc, iLocal);
            il.Emit(OpCodes.Ldloc, valLocal);
            il.Emit(OpCodes.Call, setter);
            il.Emit(OpCodes.Ldloc, iLocal);
            il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Stloc, iLocal);
            il.Emit(OpCodes.Br, loopStart);
            il.MarkLabel(loopEnd);
        }
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

    // Import type-compatibility validators, emitted into the instance constructor after each import is
    // resolved and its .NET subtype confirmed (see ImportException for the matching rules).
    // The descriptor-aware overloads are selected explicitly by parameter list (the plain overloads remain for
    // public API compatibility); the extra string? carries the declaration's closed structural descriptor.
    static readonly MethodInfo ValidateGlobalImport = typeof(ImportException).GetMethod(nameof(ImportException.ValidateGlobal),
        [typeof(GlobalImport), typeof(WebAssemblyValueType), typeof(bool), typeof(string), typeof(string), typeof(string)])!;
    static readonly MethodInfo ValidateMemoryImport = typeof(ImportException).GetMethod(nameof(ImportException.ValidateMemory))!;
    static readonly MethodInfo ValidateTableImport = typeof(ImportException).GetMethod(nameof(ImportException.ValidateTable),
        [typeof(TableImport), typeof(uint), typeof(uint), typeof(bool), typeof(bool), typeof(string), typeof(string), typeof(string)])!;

    // Pushes a closed structural descriptor argument for the descriptor-aware Validate* overloads: the string when
    // the import is reference-typed, or null (which keeps the coarse CLR-type fallback in force) when it is numeric.
    static void EmitDescriptor(ILGenerator il, string? descriptor)
    {
        if (descriptor is null)
            il.Emit(OpCodes.Ldnull);
        else
            il.Emit(OpCodes.Ldstr, descriptor);
    }

    // The (offset, length) constructor used to trap an out-of-bounds active data segment, matching the
    // exception the runtime's RangeCheck helpers throw for ordinary memory accesses.
    static readonly ConstructorInfo MemoryAccessOutOfRangeExceptionConstructor =
        typeof(MemoryAccessOutOfRangeException)
        .GetTypeInfo()
        .DeclaredConstructors
        .First(c =>
        {
            var parameters = c.GetParameters();
            return parameters.Length == 2
                && parameters[0].ParameterType == typeof(uint)
                && parameters[1].ParameterType == typeof(uint);
        });

    // The (ulong, ulong) counterpart, used to trap an out-of-bounds active data segment targeting a memory64.
    static readonly ConstructorInfo MemoryAccessOutOfRangeException64Constructor =
        typeof(MemoryAccessOutOfRangeException)
        .GetTypeInfo()
        .DeclaredConstructors
        .First(c =>
        {
            var parameters = c.GetParameters();
            return parameters.Length == 2
                && parameters[0].ParameterType == typeof(ulong)
                && parameters[1].ParameterType == typeof(ulong);
        });

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
        ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

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
        // Module-defined (not imported) memory fields, in index order; each is disposed when the instance is disposed.
        var definedMemories = new List<FieldBuilder>();

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

                        // A typed reference may target a concrete type index; that index must refer to a defined type.
                        // (Forward references within the section are legal, so this is checked once the count is known.)
                        foreach (var signature in signatures)
                        {
                            foreach (var valType in signature.RawParameterTypes)
                                ValidateTypeIndex(valType, signatures.Length, reader.Offset);
                            foreach (var valType in signature.RawReturnTypes)
                                ValidateTypeIndex(valType, signatures.Length, reader.Offset);
                        }
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
                    context.ImportedGlobals = importedGlobals;
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

                        // Only a non-empty function section needs the type section; an empty one (no entries
                        // beyond imports) is valid even when no Type section is present.
                        if (functionSignatures.Length > importedFunctionCount && signatures == null)
                            throw new InvalidOperationException();

                        for (var i = importedFunctionCount; i < functionSignatures.Length; i++)
                        {
                            var signature = functionSignatures[i] = signatures![reader.ReadVarUInt32()];
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

                            // WebAssembly 3.0 table-with-initializer: a leading 0x40 (s7 -0x40, impossible as a
                            // reftype) then a reserved 0x00 selects "0x40 0x00 tabletype constexpr". The classic
                            // encoding is just "elementtype limits".
                            var hasInit = false;
                            var lead = reader.ReadVarInt7();
                            if (lead == -0x40)
                            {
                                _ = reader.ReadByte(); // reserved 0x00
                                lead = reader.ReadVarInt7();
                                hasInit = true;
                            }

                            var elementValType = ReadTableElementTypeFromLead(reader, lead, preElementTypeOffset);
                            ValidateTypeIndex(elementValType, signatures?.Length ?? 0, preElementTypeOffset);

                            var limits = new ResizableLimits(reader);
                            var tableInit = hasInit ? Instruction.ParseInitializerExpression(reader).ToList() : null;

                            EmitTableDefinition(context, exportsBuilder, instanceConstructorIL, configuration,
                                ref functionTable, elementValType, limits, tableInit, preElementTypeOffset, EnsureFunctionReferences);
                        }
                    }
                    break;

                case Section.Memory:
                    {
                        // WebAssembly 3.0 permits more than one memory; each entry is appended to the context's
                        // memory list (after any imported memories) so instructions can select it by index.
                        var count = reader.ReadVarUInt32();
                        for (var m = 0u; m < count; m++)
                        {
                            var setFlags = (ResizableLimits.Flags)reader.ReadVarUInt32();
                            var is64 = (setFlags & ResizableLimits.Flags.Is64Bit) != 0;
                            // A 32-bit memory's size in pages must fit in 4GiB: at most 65536 pages of 64KiB each.
                            // A memory64 may declare up to 2^48 pages (its architectural limit), though the backing
                            // store still bounds what can actually be allocated. Reading at 64-bit width lets a 32-bit
                            // memory declaring 2^32 pages be rejected rather than silently truncated.
                            var memoryPagesLimit = is64 ? 0x1_0000_0000_0000UL : 0x1_0000UL;
                            var preMinimumOffset = reader.Offset;
                            var declaredMinimum = reader.ReadVarUInt64();
                            if (declaredMinimum > memoryPagesLimit)
                                throw new ModuleLoadException($"Memory size minimum of {declaredMinimum} pages must be at most {memoryPagesLimit} pages.", preMinimumOffset);
                            memoryPagesMinimum = (uint)Math.Min(declaredMinimum, uint.MaxValue / Memory.PageSize);
                            if ((setFlags & ResizableLimits.Flags.Maximum) != 0)
                            {
                                var preMaximumOffset = reader.Offset;
                                var declaredMaximum = reader.ReadVarUInt64();
                                if (declaredMaximum > memoryPagesLimit)
                                    throw new ModuleLoadException($"Memory size maximum of {declaredMaximum} pages must be at most {memoryPagesLimit} pages.", preMaximumOffset);
                                if (declaredMaximum < declaredMinimum)
                                    throw new ModuleLoadException($"Memory size minimum of {declaredMinimum} must not be greater than maximum of {declaredMaximum}.", preMaximumOffset);
                                // The backing store addresses memory with a uint, one byte short of a full 4GiB, so the
                                // last allocatable page is clamped off even though the larger value validates per spec.
                                memoryPagesMaximum = (uint)Math.Min(declaredMaximum, uint.MaxValue / Memory.PageSize);
                            }
                            else
                                memoryPagesMaximum = uint.MaxValue / Memory.PageSize;

                            var memoryField = CreateMemoryField(exportsBuilder, configuration, context.Memories.Count);
                            context.Memories.Add(memoryField);
                            context.MemoryIs64.Add(is64);
                            definedMemories.Add(memoryField);
                            memory ??= memoryField;

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

                            // Record the address type on the memory so an importer of this (exported) memory can
                            // reject an i32/i64 mismatch. Emitted only for memory64, leaving 32-bit modules unchanged.
                            EmitSetIs64(instanceConstructorIL, configuration, typeof(UnmanagedMemory), is64);

                            instanceConstructorIL.Emit(OpCodes.Stfld, memoryField);
                        }
                    }
                    break;

                case Section.Global:
                    EnsureFunctionReferences(); // Global initializers may use ref.func.
                    globals = SectionGlobal(reader, context, globals, exportsBuilder, instanceConstructorIL, importedGlobals, configuration);
                    break;

                case Section.Export:
                    exportedFunctions = SectionExport(reader, context, exportsBuilder, emptyTypes, globals, configuration);
                    // Exported functions are implicitly declared as referenceable by ref.func.
                    foreach (var exported in exportedFunctions)
                        context.DeclaredFunctionReferences.Add(exported.Value);
                    break;

                case Section.Start:
                    if (internalFunctions == null)
                        throw new ModuleLoadException("Start section created without any functions.", preSectionOffset);
                    startFunction = SectionStart(reader, internalFunctions, functionSignatures!);
                    break;

                case Section.Element:
                    EnsureFunctionReferences(); // Element initializers reference functions via FunctionReferences.
                    SectionElement(reader, context, instanceConstructorIL, functionSignatures, internalFunctions, configuration, exportsBuilder);
                    break;

                case Section.Code:
                    EnsureFunctionReferences(); // Function bodies may use ref.func.
                    context.EnforceDeclaredFunctionReferences = true;
                    // functionSignatures/internalFunctions may legitimately be null for an empty code section
                    // (no Function section); SectionCode validates the body count against the defined functions.
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
                    SectionData(reader, context, instanceConstructorIL, exportsBuilder, declaredDataCount);
                    break;

                default:
                    throw new ModuleLoadException($"Unrecognized section type {(Section)id}.", preSectionOffset);
            }

            preSectionOffset = reader.Offset;
            previousSection = (Section)id;
        }

        // Module-defined memories own unmanaged buffers; a single Dispose releases every one of them. Imported
        // memories are owned by their provider and are not disposed here.
        if (definedMemories.Count != 0)
        {
            exportsBuilder.AddInterfaceImplementation(configuration.NeutralizeType(typeof(IDisposable)));

            var dispose = exportsBuilder.DefineMethod(
                "Dispose",
                MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot,
                CallingConventions.HasThis,
                configuration.NeutralizeType(typeof(void)),
                emptyTypes
                );

            var disposeMethod = configuration.NeutralizeType(typeof(UnmanagedMemory))
                .GetTypeInfo()
                .DeclaredMethods
                .First(info =>
                    info.ReturnType == configuration.NeutralizeType(typeof(void))
                    && info.GetParameters().Length == 0
                    && info.Name == nameof(UnmanagedMemory.Dispose));

            var disposeIL = dispose.GetILGenerator();
            foreach (var definedMemory in definedMemories)
            {
                disposeIL.Emit(OpCodes.Ldarg_0);
                disposeIL.Emit(OpCodes.Ldfld, definedMemory);
                disposeIL.Emit(OpCodes.Call, disposeMethod);
            }
            disposeIL.Emit(OpCodes.Ret);
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
                        var elementValType = ReadTableElementTypeFromLead(reader, reader.ReadVarInt7(), preElementTypeoffset);
                        ValidateTypeIndex(elementValType, signatures?.Length ?? 0, preElementTypeoffset);

                        var limits = new ResizableLimits(reader);

                        var isFuncTable = CompilationContext.IsFunctionReference(elementValType);
                        var tableType = configuration.NeutralizeType(isFuncTable ? typeof(FunctionTable) : typeof(ExternRefTable));
                        var tableField = exportsBuilder.DefineField($"☣ Table{context.Tables.Count}", tableType, FieldAttributes.Private | FieldAttributes.InitOnly);

                        context.Tables.Add(tableField);
                        context.TableElementTypes.Add(elementValType);
                        context.TableIs64.Add(limits.Is64Bit);
                        if (functionTable == null && isFuncTable)
                            functionTable = tableField;

                        instanceConstructorIL.Emit(OpCodes.Ldarg_0);
                        instanceConstructorIL.Emit(OpCodes.Ldarg_1);
                        instanceConstructorIL.Emit(OpCodes.Ldstr, moduleName);
                        instanceConstructorIL.Emit(OpCodes.Ldstr, fieldName);
                        instanceConstructorIL.Emit(OpCodes.Callvirt, importFinderInvoke);

                        ImportException.EmitTryCast(instanceConstructorIL, tableType, configuration);

                        // Confirm the supplied table's limits satisfy the declaration (element type was
                        // already verified by the cast above).
                        instanceConstructorIL.Emit(OpCodes.Dup);
                        instanceConstructorIL.Emit(OpCodes.Ldc_I4, (int)limits.ClampedMinimum);
                        instanceConstructorIL.Emit(OpCodes.Ldc_I4, (int)(limits.ClampedMaximum ?? 0));
                        instanceConstructorIL.Emit(limits.Maximum64.HasValue ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
                        instanceConstructorIL.Emit(limits.Is64Bit ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
                        EmitDescriptor(instanceConstructorIL, WasmTypeDescriptor.Render(elementValType, signatures));
                        instanceConstructorIL.Emit(OpCodes.Ldstr, moduleName);
                        instanceConstructorIL.Emit(OpCodes.Ldstr, fieldName);
                        instanceConstructorIL.Emit(OpCodes.Call, ValidateTableImport);

                        instanceConstructorIL.Emit(OpCodes.Stfld, tableField);
                    }
                    break;
                case ExternalKind.Memory:
                    {
                        // WebAssembly 3.0 permits multiple memories; each imported memory gets its own backing
                        // field, appended to the context's memory list ahead of any module-defined memories.
                        var limits = new ResizableLimits(reader);

                        // An imported memory's declared size must satisfy the same page limits as a defined one:
                        // 65536 pages for i32, 2^48 for memory64. This is a decode/validation-time check independent
                        // of whether an actual memory is later supplied.
                        var importMemoryPagesLimit = limits.Is64Bit ? 0x1_0000_0000_0000UL : 0x1_0000UL;
                        if (limits.Minimum64 > importMemoryPagesLimit)
                            throw new ModuleLoadException($"Memory size minimum of {limits.Minimum64} pages must be at most {importMemoryPagesLimit} pages.", reader.Offset);
                        if (limits.Maximum64 > importMemoryPagesLimit)
                            throw new ModuleLoadException($"Memory size maximum of {limits.Maximum64} pages must be at most {importMemoryPagesLimit} pages.", reader.Offset);

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

                        var memoryField = CreateMemoryField(exportsBuilder, configuration, context.Memories.Count);
                        context.Memories.Add(memoryField);
                        context.MemoryIs64.Add(limits.Is64Bit);
                        memory ??= memoryField;
                        instanceConstructorIL.Emit(OpCodes.Ldarg_0);
                        instanceConstructorIL.Emit(OpCodes.Ldarg_0);
                        instanceConstructorIL.Emit(OpCodes.Call, importedMemoryProvider);

                        // Confirm the supplied memory's limits satisfy the declaration before storing it.
                        instanceConstructorIL.Emit(OpCodes.Dup);
                        instanceConstructorIL.Emit(OpCodes.Ldc_I4, (int)limits.ClampedMinimum);
                        instanceConstructorIL.Emit(OpCodes.Ldc_I4, (int)(limits.ClampedMaximum ?? 0));
                        instanceConstructorIL.Emit(limits.Maximum64.HasValue ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
                        instanceConstructorIL.Emit(limits.Is64Bit ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
                        instanceConstructorIL.Emit(OpCodes.Ldstr, moduleName);
                        instanceConstructorIL.Emit(OpCodes.Ldstr, fieldName);
                        instanceConstructorIL.Emit(OpCodes.Call, ValidateMemoryImport);

                        instanceConstructorIL.Emit(OpCodes.Stfld, memoryField);
                    }
                    break;
                case ExternalKind.Global:
                    {
                        var preContentOffset = reader.Offset;
                        var contentValueType = ReferenceType.ReadValueType(reader, out var contentRefType);
                        var contentValType = ValType.FromValueType(contentValueType, contentRefType);
                        ValidateTypeIndex(contentValType, signatures?.Length ?? 0, preContentOffset);
                        var contentType = contentValType.ToNarrow();
                        var contentClrType = configuration.NeutralizeType(contentValType.ToSystemType());
                        var mutable = reader.ReadVarUInt1() == 1;

                        var typedDelegate = configuration.NeutralizeType(typeof(Func<>).MakeGenericType([contentValType.ToSystemType()]));
                        var delField = $"➡ Get {moduleName}::{fieldName}";
                        var delFieldBuilder = exportsBuilder.DefineField(delField, typedDelegate, PrivateReadonlyField);

                        var getterInvoker = exportsBuilder.DefineMethod(
                            $"Invoke {delField}",
                            InternalFunctionAttributes,
                            CallingConventions.Standard,
                            contentClrType,
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

                        // Confirm the supplied global's value type and mutability match the declaration.
                        instanceConstructorIL.Emit(OpCodes.Dup);
                        instanceConstructorIL.Emit(OpCodes.Ldc_I4, (int)contentType);
                        instanceConstructorIL.Emit(mutable ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
                        EmitDescriptor(instanceConstructorIL, WasmTypeDescriptor.Render(contentValType, signatures));
                        instanceConstructorIL.Emit(OpCodes.Ldstr, moduleName);
                        instanceConstructorIL.Emit(OpCodes.Ldstr, fieldName);
                        instanceConstructorIL.Emit(OpCodes.Call, ValidateGlobalImport);

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
                            typedDelegate = configuration.NeutralizeType(typeof(Action<>).MakeGenericType([contentValType.ToSystemType()]));
                            delField = $"➡ Set {moduleName}::{fieldName}";
                            delFieldBuilder = exportsBuilder.DefineField(delField, typedDelegate, PrivateReadonlyField);

                            setterInvoker = exportsBuilder.DefineMethod(
                            $"Invoke {delField}",
                            InternalFunctionAttributes,
                            CallingConventions.Standard,
                            null,
                            [contentClrType, exportsBuilder]
                            );

                            invokerIL = setterInvoker.GetILGenerator();
                            invokerIL.EmitLoadArg(1);
                            invokerIL.Emit(OpCodes.Ldfld, delFieldBuilder);
                            invokerIL.EmitLoadArg(0);
                            invokerIL.Emit(OpCodes.Callvirt, typedDelegate.GetRuntimeMethod(nameof(Action<WebAssemblyValueType>.Invoke), [contentClrType])!);
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

                        globalImports.Add(new GlobalInfo(contentValType, true, getterInvoker, setterInvoker));
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
            var preContentOffset = reader.Offset;
            var contentValueType = ReferenceType.ReadValueType(reader, out var contentRefType);
            var contentValType = ValType.FromValueType(contentValueType, contentRefType);
            ValidateTypeIndex(contentValType, context.Types?.Length ?? 0, preContentOffset);
            var contentClrType = configuration.NeutralizeType(contentValType.ToSystemType());
            var isMutable = reader.ReadVarUInt1() == 1;

            // A global initializer (a constant expression) may reference any previously-declared immutable global:
            // the imported globals plus the module-defined globals already processed in earlier iterations.
            context.ConstantExpressionGlobalBound = importedGlobals + i;

            // The getter always takes the exports instance as its (only) argument, even for an immutable
            // global whose value is a plain constant. An immutable initializer may itself be `global.get`
            // of an imported global, which needs the instance to reach that import's backing delegate;
            // since the instance occupies the argument slot at index = parameter-count (0 here), emitting
            // it unconditionally keeps EmitLoadThis valid. Callers push the instance whenever
            // GlobalInfo.RequiresInstance is set, which is now always true for module-defined globals.
            var getter = exportsBuilder.DefineMethod(
                $"🌍 Get {i}",
                InternalFunctionAttributes,
                CallingConventions.Standard,
                contentClrType,
                [exportsBuilder]
                );

            var il = getter.GetILGenerator();
            var getterSignature = new Signature(contentValType, configuration);
            MethodBuilder? setter;

            if (!isMutable)
            {
                context.Reset(
                    il,
                    getterSignature,
                    getterSignature.RawParameterTypes
                    );

                context.ConstantExpression = true;
                foreach (var instruction in Instruction.ParseInitializerExpression(reader))
                {
                    if (instruction is Instructions.RefFunc rfGlobal)
                        context.DeclaredFunctionReferences.Add(rfGlobal.Index); // A ref.func in a global initializer declares that function.
                    instruction.Compile(context);
                    context.Previous = instruction.OpCode;
                }
                context.ConstantExpression = false;

                setter = null;
            }
            else //Mutable
            {
                var field = exportsBuilder.DefineField(
                    $"🌍 {i}",
                    contentClrType,
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
                    [contentClrType, exportsBuilder]
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

                context.ConstantExpression = true;
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
                context.ConstantExpression = false;
            }

            globals[importedGlobals + i] = new GlobalInfo(contentValType, true, getter, setter);
        }

        return globals;
    }

    static KeyValuePair<string, uint>[] SectionExport(
        Reader reader,
        CompilationContext context,
        TypeBuilder exportsBuilder,
        Type[] emptyTypes,
        GlobalInfo[]? globals,
        CompilerConfiguration configuration)
    {
        const MethodAttributes exportedPropertyAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.Virtual | MethodAttributes.Final;
        var totalExports = reader.ReadVarUInt32();
        var xFunctions = new List<KeyValuePair<string, uint>>((int)Math.Min(int.MaxValue, totalExports));
        var exportNames = new HashSet<string>(StringComparer.Ordinal);

        for (var i = 0; i < totalExports; i++)
        {
            var preNameOffset = reader.Offset;
            var name = reader.ReadString(reader.ReadVarUInt32());
            if (!exportNames.Add(name))
                throw new ModuleLoadException($"Duplicate export name \"{name}\".", preNameOffset);
            // Member (property/accessor) names must be valid identifiers; table/memory/global exports use this
            // cleaned form just as function exports do. The raw name is kept for the NativeExportAttribute so
            // re-import lookup still matches the original WASM field name.
            var memberName = NameCleaner.CleanName(name);
            var kind = (ExternalKind)reader.ReadByte();
            var preIndexOffset = reader.Offset;
            var index = reader.ReadVarUInt32();
            switch (kind)
            {
                case ExternalKind.Function:
                    xFunctions.Add(new KeyValuePair<string, uint>(name, index));
                    break;
                case ExternalKind.Table:
                    if (index >= (uint)context.Tables.Count)
                        throw new ModuleLoadException($"Exported table index {index} exceeds the {context.Tables.Count} table(s) defined or imported.", preIndexOffset);

                    {
                        var tableField = context.GetTable(index);
                        var tableClrType = configuration.NeutralizeType(
                            context.IsFuncTable(index) ? typeof(FunctionTable) : typeof(ExternRefTable));
                        var tableGetter = exportsBuilder.DefineMethod("get_" + memberName,
                            exportedPropertyAttributes,
                            CallingConventions.HasThis,
                            tableClrType,
                            emptyTypes
                            );
                        var getterIL = tableGetter.GetILGenerator();
                        getterIL.Emit(OpCodes.Ldarg_0);
                        getterIL.Emit(OpCodes.Ldfld, tableField);
                        getterIL.Emit(OpCodes.Ret);

                        var tableProperty = exportsBuilder.DefineProperty(memberName, PropertyAttributes.None, tableClrType, emptyTypes);
                        // The attribute must sit on the property (not the getter): RuntimeImport.FromCompiledExports
                        // looks up table/memory/global exports by PropertyInfo when re-importing into another instance.
#if NET9_0_OR_GREATER
                        if (configuration is not PersistedCompilerConfiguration) // Need to redesign this for persisted.
#endif
                        tableProperty.SetCustomAttribute(NativeExportAttribute.Emit(ExternalKind.Table, name,
                            WasmTypeDescriptor.Render(context.TableElementValType(index), context.Types)));
                        tableProperty.SetGetMethod(tableGetter);
                    }
                    break;
                case ExternalKind.Memory:
                    if (index >= (uint)context.Memories.Count)
                        throw new ModuleLoadException($"Exported memory index {index} exceeds the {context.Memories.Count} memory(ies) defined or imported.", preIndexOffset);

                    {
                        var memoryField = context.Memories[(int)index];
                        var memoryGetter = exportsBuilder.DefineMethod("get_" + memberName,
                            exportedPropertyAttributes,
                            CallingConventions.HasThis,
                            configuration.NeutralizeType(typeof(UnmanagedMemory)),
                            emptyTypes
                            );
                        var getterIL = memoryGetter.GetILGenerator();
                        getterIL.Emit(OpCodes.Ldarg_0);
                        getterIL.Emit(OpCodes.Ldfld, memoryField);
                        getterIL.Emit(OpCodes.Ret);

                        var memoryProperty = exportsBuilder.DefineProperty(memberName, PropertyAttributes.None, configuration.NeutralizeType(typeof(UnmanagedMemory)), emptyTypes);
                        // The attribute must sit on the property (not the getter): RuntimeImport.FromCompiledExports
                        // looks up table/memory/global exports by PropertyInfo when re-importing into another instance.
#if NET9_0_OR_GREATER
                        if (configuration is not PersistedCompilerConfiguration) // Need to redesign this for persisted.
#endif
                        memoryProperty.SetCustomAttribute(NativeExportAttribute.Emit(ExternalKind.Memory, name));
                        memoryProperty.SetGetMethod(memoryGetter);
                    }
                    break;
                case ExternalKind.Global:
                    if (globals == null)
                        throw new ModuleLoadException($"Exported index {index} is global but no globals are defined.", preIndexOffset);
                    if (index >= globals.Length)
                        throw new ModuleLoadException($"Exported global index of {index} is greater than the number of globals {globals.Length}.", preIndexOffset);

                    {
                        var global = globals[index];
                        var property = exportsBuilder.DefineProperty(memberName, PropertyAttributes.None, configuration.NeutralizeType(global.Type.ToSystemType()), emptyTypes);
#if NET9_0_OR_GREATER
                        if (configuration is not PersistedCompilerConfiguration) // Need to redesign this for persisted.
#endif
                        property.SetCustomAttribute(NativeExportAttribute.Emit(ExternalKind.Global, name,
                            WasmTypeDescriptor.Render(global.Type, context.Types)));
                        var wrappedGet = exportsBuilder.DefineMethod("get_" + memberName,
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
                            var wrappedSet = exportsBuilder.DefineMethod("set_" + memberName,
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

    static MethodInfo SectionStart(Reader reader, MethodInfo[] internalFunctions, Signature[] functionSignatures)
    {
        var preReadOffset = reader.Offset;
        var startIndex = reader.ReadVarInt32();
        if (startIndex >= internalFunctions.Length)
            throw new ModuleLoadException($"Start function of index {startIndex} exceeds available functions of {internalFunctions.Length}", preReadOffset);

        // The start function must take no parameters and return nothing.
        var signature = functionSignatures[startIndex];
        if (signature.ParameterTypes.Length != 0 || signature.ReturnTypes.Length != 0)
            throw new ModuleLoadException($"Start function of index {startIndex} must have no parameters and no results, found {signature.ParameterTypes.Length} parameter(s) and {signature.ReturnTypes.Length} result(s).", preReadOffset);

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
    static void ValidateElementInitExpr(IList<Instruction> initExpr, ValType elemType, CompilationContext context)
    {
        // A reference-type element initializer is a constant expression yielding exactly one reference value, then end.
        if (initExpr.Count != 2 || initExpr[1] is not Instructions.End)
            throw new ModuleLoadException("An element initializer expression must be a single reference-producing constant expression followed by end.", 0);

        ValType produced;
        switch (initExpr[0])
        {
            case Instructions.RefFunc refFunc:
            {
                var sigs = context.CheckedFunctionSignatures;
                if (refFunc.Index >= (uint)sigs.Length)
                    throw new ModuleLoadException($"unknown function {refFunc.Index}: the module declares only {sigs.Length} function(s).", 0);
                produced = ValType.RefIndexed(false, sigs[refFunc.Index].TypeIndex); // (ref $sig(f))
                break;
            }
            case Instructions.RefNull refNull:
                produced = refNull.TypeIndex is { } idx
                    ? ValType.RefIndexed(true, idx)
                    : ValType.Ref(true, refNull.HeapType ?? WebAssemblyHeapType.Func);
                break;
            case Instructions.GlobalGet globalGet:
            {
                var globals = context.Globals ?? [];
                if (globalGet.Index >= (uint)globals.Length)
                    throw new ModuleLoadException($"unknown global {globalGet.Index}.", 0);
                produced = globals[globalGet.Index].Type;
                break;
            }
            default:
                throw new ModuleLoadException($"Element initializer expression ({initExpr[0].OpCode}) is not a supported constant reference expression.", 0);
        }

        // The initializer's produced reference type must be a subtype of the segment's element type.
        if (!context.IsSubtype(produced, elemType))
            throw new ModuleLoadException($"type mismatch: element initializer produces {produced.ToNarrow()}, not a subtype of segment element type {elemType.ToNarrow()}.", 0);
    }

    static void EmitElementRefValue(ILGenerator il, CompilationContext context, IList<Instruction> initExpr)
    {
        // Each per-element initializer is a constant expression of the form [ref.func N, end], [ref.null t, end],
        // or [global.get N, end].
        if (initExpr.Count >= 1 && initExpr[0] is Instructions.RefFunc refFunc)
        {
            context.DeclaredFunctionReferences.Add(refFunc.Index);
            il.EmitLoadArg(0);
            il.Emit(OpCodes.Ldfld, context.FunctionReferences!);
            il.EmitLoadConstant(refFunc.Index);
            il.Emit(OpCodes.Ldelem_Ref);
        }
        else if (initExpr.Count >= 1 && initExpr[0] is Instructions.GlobalGet globalGet)
        {
            // The referenced (imported, immutable) global holds the reference value to store. Its getter
            // return type is Delegate (funcref) or object (externref), matching the destination table's setter.
            var global = context.Globals![globalGet.Index];
            if (global.RequiresInstance)
                il.EmitLoadArg(0);
            il.Emit(OpCodes.Call, global.Getter);
        }
        else
        {
            il.Emit(OpCodes.Ldnull);
        }
    }

    static void SectionElement(Reader reader, CompilationContext context, ILGenerator instanceConstructorIL, Signature[]? functionSignatures, MethodInfo[]? internalFunctions, CompilerConfiguration configuration, TypeBuilder exportsBuilder)
    {
        var count = reader.ReadVarUInt32();

        // An active segment's offset is a constant expression compiled into the instance constructor, so it
        // can be a global.get of an imported global as well as an i32.const. The result is held in this local.
        context.Reset(instanceConstructorIL, Signature.Empty, Signature.Empty.RawParameterTypes);
        var offsetLocal = instanceConstructorIL.DeclareLocal(typeof(uint));
        LocalBuilder? offsetLocal64 = null;

        for (var i = 0u; i < count; i++)
        {
            var element = new Element(reader);
            var isActive = element.IsActive;
            var isDeclarative = element.IsDeclarative;
            var usesInitExprs = element.UsesExpressions;

            // The segment's element reference type: a WebAssembly 3.0 long-form typed reference when present,
            // otherwise the shorthand elem type. The func-index form's elements are non-null function references
            // (`ref.func`), so its element type is (ref func) — the type that is a subtype of both funcref and any
            // typed (ref func)-family table element type (elem.2 initializes a `(ref func)` table this way).
            var elemValType = element.ElemRefType is { } elemRefType
                ? ValType.FromReferenceType(elemRefType)
                : usesInitExprs
                    ? ValType.FromValueType((WebAssemblyValueType)(sbyte)element.ElemType)
                    : ValType.Ref(false, WebAssemblyHeapType.Func);
            ValidateTypeIndex(elemValType, context.Types?.Length ?? 0, 0);
            var elemIsFunc = CompilationContext.IsFunctionReference(elemValType);
            var entryCount = usesInitExprs ? element.InitExprs.Count : element.Elements.Count;

            // Register declared function references (func-index forms); init-expr forms register in EmitElementRefValue.
            if (!usesInitExprs)
                foreach (var fi in element.Elements)
                    context.DeclaredFunctionReferences.Add(fi);

            // Register a backing field for every segment so elem.drop / table.init can resolve any valid index.
            // Passive segments are populated below; active and declarative segments leave the field null (= empty/dropped).
            var segField = exportsBuilder.DefineField(
                $"☣ ElementSegment {i}",
                elemIsFunc ? typeof(Delegate[]) : typeof(object[]),
                FieldAttributes.Private);
            context.ElementSegments[i] = segField;
            context.ElementSegmentTypes[i] = elemValType;

            // Each reference-producing initializer expression must yield a value matching the segment's element type.
            if (usesInitExprs)
                foreach (var initExpr in element.InitExprs)
                    ValidateElementInitExpr(initExpr, elemValType, context);

            if (isActive)
            {
                var tableIndex = element.Index;
                if (tableIndex >= (uint)context.Tables.Count)
                    throw new ModuleLoadException($"Element segment references table {tableIndex} but only {context.Tables.Count} tables exist.", 0);

                // The active segment's element type must be a subtype of the destination table's element type.
                var tableElemType = context.TableElementValType(tableIndex);
                if (!context.IsSubtype(elemValType, tableElemType))
                    throw new ModuleLoadException($"type mismatch: element segment type {elemValType.ToNarrow()} is not a subtype of table {tableIndex} element type {tableElemType.ToNarrow()}.", 0);

                // For a table64 destination the offset constant expression is an i64; hold it in a ulong local and
                // run the fits check in 64-bit. A 32-bit table keeps the original i32/uint path.
                var tableIs64 = context.IsTable64(tableIndex);
                var localOffset = tableIs64 ? (offsetLocal64 ??= instanceConstructorIL.DeclareLocal(typeof(ulong))) : offsetLocal;

                // Offset is a constant expression (i32/i64.const or global.get of an imported immutable global),
                // compiled into the constructor and left in localOffset.
                new Instructions.Block(tableIs64 ? BlockType.Int64 : BlockType.Int32).Compile(context);
                context.ConstantExpression = true;
                // By section order all globals are declared before element segments, so any immutable global is in scope.
                context.ConstantExpressionGlobalBound = context.Globals?.Length ?? 0;
                foreach (var instruction in element.InitializerExpression)
                {
                    instruction.Compile(context);
                    context.Previous = instruction.OpCode;
                }
                context.ConstantExpression = false;
                context.Stack.Pop();
                context.BlockContexts.Remove(context.Depth.Count);
                instanceConstructorIL.Emit(OpCodes.Stloc, localOffset);

                var isFunc = context.IsFuncTable(tableIndex);
                var tableType = isFunc ? typeof(FunctionTable) : typeof(ExternRefTable);
                var localTable = instanceConstructorIL.DeclareLocal(tableType);
                instanceConstructorIL.EmitLoadArg(0);
                instanceConstructorIL.Emit(OpCodes.Ldfld, context.GetTable(tableIndex));
                instanceConstructorIL.Emit(OpCodes.Stloc, localTable);

                var setter = isFunc ? FunctionTable.IndexSetter : ExternRefTable.IndexSetter;

                // An active element segment that does not fit its destination table traps at instantiation.
                // (Earlier versions grew a funcref table to fit; the spec requires a trap instead.) The required
                // extent (offset + count) is compared in 64-bit to avoid a uint overflow near the 2^32 boundary.
                // This also covers an empty segment whose offset alone exceeds the table.
                var lengthGetter = isFunc ? FunctionTable.LengthGetter : ExternRefTable.LengthGetter;
                var fits = instanceConstructorIL.DefineLabel();
                instanceConstructorIL.Emit(OpCodes.Ldloc, localTable);
                instanceConstructorIL.Emit(OpCodes.Call, lengthGetter);
                instanceConstructorIL.Emit(OpCodes.Conv_U8);
                instanceConstructorIL.Emit(OpCodes.Ldloc, localOffset);
                instanceConstructorIL.Emit(OpCodes.Conv_U8); // No-op for an already-i64 (table64) offset; widens a u32 one.
                instanceConstructorIL.Emit(OpCodes.Ldc_I8, (long)entryCount);
                instanceConstructorIL.Emit(OpCodes.Add);
                instanceConstructorIL.Emit(OpCodes.Bge_Un, fits);
                instanceConstructorIL.Emit(OpCodes.Ldloc, localOffset);
                if (tableIs64)
                    instanceConstructorIL.Emit(OpCodes.Conv_U4); // The exception reports a 32-bit index; the value was out of range anyway.
                instanceConstructorIL.Emit(OpCodes.Ldloc, localTable);
                instanceConstructorIL.Emit(OpCodes.Call, lengthGetter);
                instanceConstructorIL.Emit(OpCodes.Newobj, typeof(TableAccessOutOfRangeException)
                    .GetTypeInfo()
                    .DeclaredConstructors
                    .First(c =>
                    {
                        var parms = c.GetParameters();
                        return parms.Length == 2 && parms[0].ParameterType == typeof(uint) && parms[1].ParameterType == typeof(uint);
                    }));
                instanceConstructorIL.Emit(OpCodes.Throw);
                instanceConstructorIL.MarkLabel(fits);

                for (var j = 0; j < entryCount; j++)
                {
                    instanceConstructorIL.Emit(OpCodes.Ldloc, localTable);
                    instanceConstructorIL.Emit(OpCodes.Ldloc, localOffset);
                    instanceConstructorIL.Emit(OpCodes.Ldc_I4, j);
                    if (tableIs64)
                    {
                        // offset (i64, already validated to fit) + j, narrowed to the int index the setter takes.
                        instanceConstructorIL.Emit(OpCodes.Conv_I8);
                        instanceConstructorIL.Emit(OpCodes.Add);
                        instanceConstructorIL.Emit(OpCodes.Conv_I4);
                    }
                    else
                        instanceConstructorIL.Emit(OpCodes.Add);
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
                var elementClrType = elemIsFunc ? typeof(Delegate) : typeof(object);
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

    static void SectionCode(Reader reader, CompilationContext context, Signature[]? functionSignatures, MethodInfo[]? internalFunctions, int importedFunctions)
    {
        var preBodiesIndex = reader.Offset;
        var functionBodies = reader.ReadVarUInt32();

        // Defined (non-imported) functions; zero when there is no Function section at all.
        var definedFunctions = (functionSignatures?.Length ?? importedFunctions) - importedFunctions;
        if (functionBodies != definedFunctions)
            throw new ModuleLoadException($"Code section has {functionBodies} functions described but {definedFunctions} were expected.", preBodiesIndex);

        if (functionBodies == 0)
            return; // An empty code section is valid and needs no Function/Type section.

        // functionBodies > 0 implies definedFunctions > 0, so both arrays are present.
        for (var functionBodyIndex = 0; functionBodyIndex < functionBodies; functionBodyIndex++)
        {
            var signature = functionSignatures![importedFunctions + functionBodyIndex];
            var byteLength = reader.ReadVarUInt32();
            var startingOffset = reader.Offset;

            var locals = new Local[reader.ReadVarUInt32()];
            for (var localIndex = 0; localIndex < locals.Length; localIndex++)
                locals[localIndex] = new Local(reader);

            var il = ((MethodBuilder)internalFunctions![importedFunctions + functionBodyIndex]).GetILGenerator();

            // Expand the declared locals into per-slot canonical types (each Local.Type already carries the full
            // typed-reference detail parsed above). A local's concrete-index reference must name a defined type.
            var typeCount = context.Types?.Length ?? 0;
            var localValTypes = locals
                .SelectMany(local => Enumerable.Range(0, checked((int)local.Count)).Select(_ => local.Type))
                .ToArray();
            foreach (var localValType in localValTypes)
                ValidateTypeIndex(localValType, typeCount, reader.Offset);

            context.Reset(
                il,
                signature,
                [
                    .. signature.RawParameterTypes,
                    .. localValTypes,
                ]);

            foreach (var local in localValTypes)
                il.DeclareLocal(local.ToSystemType());

            foreach (var instruction in Instruction.Parse(reader))
            {
                context.CurrentInstruction = instruction;
                instruction.Compile(context);
                context.Previous = instruction.OpCode;
            }

            if (reader.Offset - startingOffset != byteLength)
                throw new ModuleLoadException($"Instruction sequence reader ended after reading {reader.Offset - startingOffset} characters, expected {byteLength}.", reader.Offset);
        }
    }

    static void SectionData(Reader reader, CompilationContext context, ILGenerator instanceConstructorIL, TypeBuilder exportsBuilder, uint? declaredDataCount)
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

        var address = instanceConstructorIL.DeclareLocal(typeof(uint));
        LocalBuilder? address64 = null;

        for (var i = 0u; i < count; i++)
        {
            var startingOffset = reader.Offset;
            var kind = reader.ReadVarUInt32();

            if (kind == 1)
            {
                StorePassiveDataSegment(context, instanceConstructorIL, exportsBuilder, i, reader.ReadBytes(reader.ReadVarUInt32()));
                continue;
            }

            // Kind 2 is active with an explicit memory index (WebAssembly 3.0 multiple memories); kind 0 targets memory 0.
            var memoryIndex = 0u;
            if (kind == 2)
                memoryIndex = reader.ReadVarUInt32();
            else if (kind != 0)
                throw new ModuleLoadException($"Data segment kind must be 0, 1, or 2, found {kind}.", startingOffset);

            // Active data segments (kind 0 or 2) require the target memory to exist.
            if (memoryIndex >= (uint)context.Memories.Count)
                throw new ModuleLoadException(context.Memories.Count == 0
                    ? "Active data segment cannot be used unless a memory section is defined."
                    : $"unknown memory {memoryIndex}", startingOffset);
            var targetMemory = context.Memories[(int)memoryIndex];
            // For a memory64 target the offset constant expression is an i64; hold it in a ulong local and run the
            // bounds check in 64-bit arithmetic. A 32-bit target keeps the original i32/uint path.
            var is64 = context.IsMemory64(memoryIndex);
            var offsetLocal = is64 ? (address64 ??= instanceConstructorIL.DeclareLocal(typeof(ulong))) : address;

            // Prevents "end" instruction of the initializer expression from becoming a return.
            new Instructions.Block(is64 ? BlockType.Int64 : BlockType.Int32).Compile(context);
            context.ConstantExpression = true;
            // By section order all globals are declared before data segments, so any immutable global is in scope.
            context.ConstantExpressionGlobalBound = context.Globals?.Length ?? 0;
            foreach (var instruction in Instruction.ParseInitializerExpression(reader))
            {
                instruction.Compile(context);
                context.Previous = instruction.OpCode;
            }
            context.ConstantExpression = false;
            context.Stack.Pop();
            context.BlockContexts.Remove(context.Depth.Count);
            instanceConstructorIL.Emit(OpCodes.Stloc, offsetLocal);

            var data = reader.ReadBytes(reader.ReadVarUInt32());

            if (data.Length == 0)
            {
                // An empty active segment writes nothing, but its offset must still be in bounds: the spec
                // bounds check is offset + length <= memory size, which for length 0 is offset <= memory size.
                // The RangeCheck8 path below would validate offset + (length - 1) as an accessible byte, one
                // too strict here (offset == size is in bounds for an empty segment), so check the boundary
                // directly and trap with the same exception type the runtime raises for memory accesses.
                var inBounds = instanceConstructorIL.DefineLabel();
                instanceConstructorIL.Emit(OpCodes.Ldloc, offsetLocal);
                instanceConstructorIL.Emit(OpCodes.Ldarg_0);
                instanceConstructorIL.Emit(OpCodes.Ldfld, targetMemory);
                instanceConstructorIL.Emit(OpCodes.Call, UnmanagedMemory.SizeGetter);
                if (is64)
                    instanceConstructorIL.Emit(OpCodes.Conv_U8); // Compare the u64 offset against the widened size.
                instanceConstructorIL.Emit(OpCodes.Ble_Un_S, inBounds);
                instanceConstructorIL.Emit(OpCodes.Ldloc, offsetLocal);
                if (is64)
                {
                    instanceConstructorIL.Emit(OpCodes.Ldc_I4_0);
                    instanceConstructorIL.Emit(OpCodes.Conv_U8);
                    instanceConstructorIL.Emit(OpCodes.Newobj, MemoryAccessOutOfRangeException64Constructor);
                }
                else
                {
                    instanceConstructorIL.Emit(OpCodes.Ldc_I4_0);
                    instanceConstructorIL.Emit(OpCodes.Newobj, MemoryAccessOutOfRangeExceptionConstructor);
                }
                instanceConstructorIL.Emit(OpCodes.Throw);
                instanceConstructorIL.MarkLabel(inBounds);
                continue;
            }

            //Ensure sufficient memory is allocated, error if max is exceeded. RangeCheck8 validates that the
            //last written byte (address + data.Length - 1) is accessible, which permits a segment that exactly
            //fills memory to the page boundary.
            instanceConstructorIL.Emit(OpCodes.Ldloc, offsetLocal);
            if (is64)
            {
                instanceConstructorIL.Emit(OpCodes.Ldc_I4, data.Length - 1);
                instanceConstructorIL.Emit(OpCodes.Conv_U8);
            }
            else
                instanceConstructorIL.Emit(OpCodes.Ldc_I4, data.Length - 1);
            instanceConstructorIL.Emit(OpCodes.Add_Ovf_Un);

            instanceConstructorIL.Emit(OpCodes.Ldarg_0);

            instanceConstructorIL.Emit(OpCodes.Call, context.GetRangeCheck(memoryIndex, 1));
            instanceConstructorIL.Emit(OpCodes.Pop);

            if (data.Length > 0x3f0000) //Limitation of DefineInitializedData, can be corrected by splitting the data.
                throw new NotSupportedException($"Data segment {i} is length {data.Length}, exceeding the current implementation limit of 4128768.");

            var field = exportsBuilder.DefineInitializedData($"☣ Data {i}", data, FieldAttributes.Assembly | FieldAttributes.InitOnly);

            instanceConstructorIL.Emit(OpCodes.Ldarg_0);
            instanceConstructorIL.Emit(OpCodes.Ldfld, targetMemory);
            instanceConstructorIL.Emit(OpCodes.Call, UnmanagedMemory.StartGetter);
            instanceConstructorIL.Emit(OpCodes.Ldloc, offsetLocal);
            instanceConstructorIL.Emit(OpCodes.Conv_U); // Zero-extend the offset; Conv_I would sign-extend an offset >= 2^31 into a wild pointer.
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

    // Emits `dup; ldc.i4.1; callvirt set_Is64Bit` on the object at the top of the stack (a UnmanagedMemory or a
    // TableImport), recording its 64-bit address type. Only emitted when is64 is true, so 32-bit modules — which
    // leave the property at its default false — produce byte-identical IL to before.
    static void EmitSetIs64(ILGenerator il, CompilerConfiguration configuration, Type declaringType, bool is64)
    {
        if (!is64)
            return;
        var setter = configuration.NeutralizeType(declaringType)
            .GetTypeInfo()
            .GetDeclaredProperty(nameof(UnmanagedMemory.Is64Bit))!
            .SetMethod!;
        il.Emit(OpCodes.Dup);
        il.Emit(OpCodes.Ldc_I4_1);
        il.Emit(OpCodes.Callvirt, setter);
    }

    static FieldBuilder CreateMemoryField(TypeBuilder exportsBuilder, CompilerConfiguration configuration, int memoryIndex)
        => exportsBuilder.DefineField(
            memoryIndex == 0 ? "☣ Memory" : $"☣ Memory {memoryIndex}",
            configuration.NeutralizeType(typeof(UnmanagedMemory)),
            PrivateReadonlyField);
}
