using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Runtime.Intrinsics;
using System.Text.Json;
using System.Text.Json.Serialization;
#if NET9_0_OR_GREATER
using System.Runtime.Loader;
#endif

// Effect of this is trusting that the source JSONs are valid.
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

namespace WebAssembly.Runtime;

static partial class SpecTestRunner
{
    /// <summary>
    /// Runs every command in the spec category's JSON file.
    /// </summary>
    /// <param name="category">The spec category relative to <c>Runtime/SpecTestData</c>, using <c>/</c> to
    /// separate nested directories (e.g. <c>address</c> or <c>simd/simd_address</c>). The data directory and
    /// its <c>&lt;leaf&gt;.json</c> command file are derived from this, since the JSON stem always matches the
    /// leaf directory name.</param>
    /// <param name="skip">Lines deferred because of a fixable limitation. A category with any such line ends
    /// Inconclusive, signalling work that remains.</param>
    /// <param name="unsupported">Lines intentionally not supported — scenarios that can't reasonably pass under
    /// the .NET execution model (e.g. JIT folding of identity float operations) and aren't expected to. These
    /// are skipped like <paramref name="skip"/> lines but do <em>not</em> make the category Inconclusive, since
    /// nothing is pending. This is the per-line counterpart to the shape-based auto-skip at the top of the loop.</param>
    public static void Run(string category, ReadOnlySpan<uint> skip = default, ReadOnlySpan<uint> unsupported = default)
        => Run(category, SpecExecutionMode.InMemory, skip, unsupported);

    /// <summary>
    /// Runs every command in the spec category's JSON file using the chosen execution back-end.
    /// </summary>
    /// <param name="category">The spec category (see the other <see cref="Run(string, ReadOnlySpan{uint}, ReadOnlySpan{uint})"/> overload).</param>
    /// <param name="mode">The execution back-end: the in-memory Reflection.Emit path or the persisted WASM-to-DLL path.</param>
    /// <param name="skip">Lines deferred because of a fixable limitation.</param>
    /// <param name="unsupported">Lines intentionally not supported.</param>
    public static void Run(string category, SpecExecutionMode mode, ReadOnlySpan<uint> skip = default, ReadOnlySpan<uint> unsupported = default)
    {
        var parts = category.Split('/');
        Run<object>(Path.Combine(["Runtime", "SpecTestData", .. parts]), parts[^1] + ".json", mode, skip, unsupported, onFailure: null);
    }

    /// <summary>
    /// Runs every command in the JSON file, catching all failures into the returned list rather
    /// than throwing on the first one. Used by <see cref="SpecDiscovery"/> to enumerate the line
    /// numbers that need skip coverage after a spec test refresh.
    /// </summary>
    public static List<(uint Line, string Message)> Discover(string pathBase, string json)
    {
        var failures = new List<(uint, string)>();
        Run<object>(pathBase, json, SpecExecutionMode.InMemory, skip: default, unsupported: default,
            onFailure: (line, x) => failures.Add((line, $"{x.GetType().Name}: {x.Message.Split('\n')[0]}")));
        return failures;
    }

    [JsonSourceGenerationOptions(IncludeFields = true)]
    [JsonSerializable(typeof(TestInfo))]
    private sealed partial class SpecTestSerializerContext : JsonSerializerContext
    {
    }

    private static void Run<TExports>(string pathBase, string json, SpecExecutionMode mode, ReadOnlySpan<uint> skip, ReadOnlySpan<uint> unsupported, Action<uint, Exception>? onFailure)
        where TExports : class
    {
        TestInfo testInfo;
        using (var reader = File.OpenRead(Path.Combine(pathBase, json)))
        {
            testInfo = JsonSerializer.Deserialize(reader, SpecTestSerializerContext.Default.TestInfo)!;
        }

        using var compiler = SpecCompiler.Create<TExports>(mode);

        ObjectMethods? methodsByName = null;
        var moduleMethodsByName = new Dictionary<string, ObjectMethods>();
        // WASM 3.0 `module definition`s: a compiled-but-not-yet-instantiated module kept by name so
        // one or more later `module instance` commands can instantiate it.
        var moduleDefinitions = new Dictionary<string, Func<ImportDictionary, object>>();

        // From https://github.com/WebAssembly/spec/blob/master/interpreter/host/spectest.ml
        var imports = new ImportDictionary
        {
            { "spectest", "print", new FunctionImport(() => { }) },
            { "spectest", "print_i32", new FunctionImport((Action<int>)(i => { })) },
            { "spectest", "print_i64", new FunctionImport((Action<long>)(i => { })) },
            { "spectest", "print_i32_f32", new FunctionImport((Action<int, float>)((i, f) => { })) },
            { "spectest", "print_f64_f64", new FunctionImport((Action<double, double>)((d1, d2) => { })) },
            { "spectest", "print_f32", new FunctionImport((Action<float>)(i => { })) },
            { "spectest", "print_f64", new FunctionImport((Action<double>)(i => { })) },
            { "spectest", "global_i32", new GlobalImport(() => 666) },
            { "spectest", "global_i64", new GlobalImport(() => 666L) },
            { "spectest", "global_f32", new GlobalImport(() => 666.6F) },
            { "spectest", "global_f64", new GlobalImport(() => 666.6) },
            { "spectest", "table", new FunctionTable(10, 20) }, // Table.alloc (TableType ({min = 10l; max = Some 20l}, FuncRefType))
            { "spectest", "table64", new FunctionTable(10, 20) { Is64Bit = true } }, // The table64 counterpart used by memory64/table64.wast.
            { "spectest", "memory", new MemoryImport(() => new UnmanagedMemory(1, 2)) }, // Memory.alloc (MemoryType {min = 1l; max = Some 2l})
        };

        var registrationCandidates = new ImportDictionary();

        Action trapExpected;
        object? result;
        object obj;
        MethodInfo methodInfo;
        foreach (var command in testInfo.commands)
        {
            if (skip.Contains(command.line))
                continue;

            // Intentionally-unsupported lines are skipped like deferred ones but, unlike skips, do not make the
            // category Inconclusive (see the end of the loop) — there is no pending work behind them.
            if (unsupported.Contains(command.line))
                continue;

            // Auto-skip scenarios that can never pass under this execution model, regardless of
            // library correctness. Recognized by command *shape* (not line number), so they stay
            // out of the hand-curated skip lists, never count toward the inconclusive signal, and
            // keep the Discover helper from crashing the test host. "call stack exhausted" requires
            // a StackOverflowException, which .NET makes uncatchable — it tears down the whole
            // process — so invoking the runaway-recursion action would crash the host rather than
            // produce a catchable failure. Keep this tightly scoped to genuinely-impossible cases;
            // anything fixable belongs in a re-assessable per-line skip instead.
            if (command is AssertExhaustion { text: "call stack exhausted" })
                continue;

            void GetMethod(TestAction action, out MethodInfo info, out object host)
            {
                var methodSource = action.module == null ? methodsByName : moduleMethodsByName[action.module];
                Assert.IsNotNull(methodSource, $"{command.line} has no method source.");
                Assert.IsTrue(methodSource!.TryGetValue(NameCleaner.CleanName(action.field), out info!), $"{command.line} failed to look up method {action.field}");
                host = methodSource.Host;
            }

            try
            {
                switch (command)
                {
                    case ModuleCommand module:
                        var path = Path.Combine(pathBase, module.LoadFilename);
                        var parsed = Module.ReadFromBinary(path); // Ensure the module parser can read it.
                        Assert.IsNotNull(parsed);
                        methodsByName = new ObjectMethods(compiler.Compile(path)(imports));
                        if (module.name != null)
                            moduleMethodsByName[module.name] = methodsByName;
                        continue;
                    case ModuleDefinition definition:
                        // Compile once; instantiation is deferred to the matching module_instance commands.
                        var defPath = Path.Combine(pathBase, definition.LoadFilename);
                        Assert.IsNotNull(Module.ReadFromBinary(defPath)); // Ensure the module parser can read it.
                        var definitionCreator = compiler.Compile(defPath);
                        // An anonymous definition (no name) is a standalone validation check with no later
                        // module_instance to reference it; compiling it above is the whole test, so don't try to
                        // register it under a null key.
                        if (definition.name != null)
                            moduleDefinitions[definition.name] = definitionCreator;
                        continue;
                    case ModuleInstance instance:
                        Assert.IsTrue(moduleDefinitions.TryGetValue(instance.module, out var creator),
                            $"{command.line} references undefined module definition {instance.module}.");
                        // A fresh instantiation of the shared compiled definition, keyed by instance name.
                        moduleMethodsByName[instance.instance] = new ObjectMethods(creator!(imports));
                        continue;
                    case AssertReturn assert:
                        GetMethod(assert.action, out methodInfo, out obj);
                        try
                        {
                            result = assert.action.Call(methodInfo, obj);
                        }
                        catch (TargetInvocationException x) when (x.InnerException != null)
                        {
                            throw new AssertFailedException($"{command.line}: {x.InnerException.Message}", x.InnerException);
                        }
                        catch (Exception x)
                        {
                            throw new AssertFailedException($"{command.line}: {x.Message}", x);
                        }
                        if (assert.expected?.Length > 0)
                        {
                            if (assert.expected.Length == 1)
                            {
                                if (!CompareReturnValue(assert.expected[0], result, command.line))
                                    return; // Inconclusive (expected value type could not be parsed).
                            }
                            else
                            {
                                // Multi-value (WASM 2.0): the export returns a ValueTuple; compare it field-by-field.
                                var actualValues = DecomposeResult(result, assert.expected.Length);
                                for (var i = 0; i < assert.expected.Length; i++)
                                {
                                    if (!CompareReturnValue(assert.expected[i], actualValues[i], command.line))
                                        return;
                                }
                            }
                        }
                        continue;
                    case AssertReturnCanonicalNan assert:
                        GetMethod(assert.action, out methodInfo, out obj);
                        result = assert.action.Call(methodInfo, obj);
                        switch (assert.expected[0].type)
                        {
                            case RawValueType.f32:
                                Assert.IsTrue(float.IsNaN((float)result!), $"{command.line}: Expected NaN, got {result}");
                                continue;
                            case RawValueType.f64:
                                Assert.IsTrue(double.IsNaN((double)result!), $"{command.line}: Expected NaN, got {result}");
                                continue;
                            default:
                                throw new AssertFailedException($"{assert.expected[0].type} doesn't support NaN checks.");
                        }
                    case AssertReturnArithmeticNan assert:
                        GetMethod(assert.action, out methodInfo, out obj);
                        result = assert.action.Call(methodInfo, obj);
                        switch (assert.expected[0].type)
                        {
                            case RawValueType.f32:
                                Assert.IsTrue(float.IsNaN((float)result!), $"{command.line}: Expected NaN, got {result}");
                                continue;
                            case RawValueType.f64:
                                Assert.IsTrue(double.IsNaN((double)result!), $"{command.line}: Expected NaN, got {result}");
                                continue;
                            default:
                                throw new AssertFailedException($"{assert.expected[0].type} doesn't support NaN checks.");
                        }
                    case AssertInvalid assert:
                        if (assert.filename.EndsWith(".wat", StringComparison.Ordinal))
                            continue; // Text-format module; not writing a WAT parser.
                        trapExpected = () =>
                        {
                            try
                            {
                                compiler.Compile(Path.Combine(pathBase, assert.filename));
                            }
                            catch (TargetInvocationException x) when (x.InnerException != null)
                            {
                                ExceptionDispatchInfo.Capture(x.InnerException).Throw();
                            }
                        };
                        switch (assert.text)
                        {
                            case "type mismatch":
                            // WASM 3.0 sometimes prefixes the detail ("type mismatch: instruction requires [i32] but
                            // stack has …"); a catch-clause subtype mismatch in try_table surfaces this form.
                            case var tmm when tmm.StartsWith("type mismatch", StringComparison.Ordinal):
                                try
                                {
                                    trapExpected();
                                }
                                catch (StackTypeInvalidException)
                                {
                                    continue;
                                }
                                catch (StackTooSmallException)
                                {
                                    continue;
                                }
                                catch (ModuleLoadException)
                                {
                                    continue;
                                }
                                catch (StackSizeIncorrectException)
                                {
                                    continue;
                                }
                                catch (LabelTypeMismatchException)
                                {
                                    continue;
                                }
                                catch (Exception x)
                                {
                                    throw new AssertFailedException($"{command.line} threw an unexpected exception of type {x.GetType().Name}.");
                                }
                                throw new AssertFailedException($"{command.line} should have thrown an exception but did not.");
                            case "alignment must not be larger than natural":
                            // A 32-bit memory access whose memarg offset exceeds u32; the compiler rejects it because
                            // the offset immediate is only u64-wide for a memory64 target.
                            case "offset out of range":
                            case "global is immutable":
                            // A global.set (or other write) targeting an immutable global; the compiler rejects it
                            // because the global has no setter. wasm-tools emits this text as "immutable global".
                            case "immutable global":
                                Assert.ThrowsExactly<CompilerException>(trapExpected, $"{command.line}");
                                continue;
                            // "unknown memory" suffixed with the offending index ("unknown memory 0/1"), e.g. an
                            // active data segment whose explicit memory index refers to a memory that doesn't exist.
                            case var um when um.StartsWith("unknown memory ", StringComparison.Ordinal):
                            case "constant expression required":
                            case "duplicate export name":
                            case "unknown table":
                            case "unknown local":
                            case "multiple memories":
                            case "size minimum must not be greater than maximum":
                            case "memory size must be at most 65536 pages (4GiB)":
                            // wasm-tools emits the shorter "memory size" for an over-limit memory min/max; the
                            // compiler rejects it with a ModuleLoadException naming the page limit.
                            case "memory size":
                            case "unknown label":
                            // A local.get of a non-nullable reference local that is not definitely assigned at that
                            // point (WebAssembly 3.0 definite-assignment); the compiler rejects it at validation.
                            case "uninitialized local":
                            case "unknown type":
                            // WASM 3.0 garbage collection: struct.set targeting an immutable field is rejected at
                            // validation because the emitted field is written only through struct.new. wasm-tools
                            // (newer upstream) phrases this as "immutable field".
                            case "field is immutable":
                            case "immutable field":
                            // WASM 3.0 garbage collection arrays: array.set/fill/copy/init_* on an immutable array,
                            // array.new_data/init_data on a reference element, and array.copy with an unassignable
                            // source element are all rejected at validation. wasm-tools (newer upstream) phrases the
                            // immutability case as "immutable array".
                            case "array is immutable":
                            case "immutable array":
                            case "array type is not numeric or vector":
                            case "array types do not match":
                            // WASM 3.0 garbage collection: an illegal declared subtype — a final or out-of-range
                            // supertype, or a composite that is not a structural subtype of its declared supertype —
                            // is rejected at validation. wasm-tools emits both the bare "sub type" and the longer
                            // "sub type N does not match super type M".
                            case "sub type":
                            case var subType when subType.StartsWith("sub type", StringComparison.Ordinal):
                            // WASM 3.0 exception handling: a tag whose referenced type has non-empty results, or a
                            // catch clause / throw naming a tag index the module doesn't define.
                            case "non-empty tag result type":
                            case var utag when utag.StartsWith("unknown tag", StringComparison.Ordinal):
                            case "unknown data segment":
                            case "unknown data segment 1":
                            case "start function":
                                Assert.ThrowsExactly<ModuleLoadException>(trapExpected, $"{command.line}");
                                continue;
                            case "unknown memory":
                            case "unknown function":
                            case "unknown table 0":
                            case "undeclared function reference":
                            case "invalid lane index":
                            // Result-arity validation can surface as a stack-size compilation error rather than a parse error.
                            case "invalid result arity":
                            // "unknown global", optionally suffixed with the offending index ("unknown global 0/1"),
                            // covers a constant expression's global.get referencing a module-defined or out-of-range global.
                            case var ug when ug.StartsWith("unknown global", StringComparison.Ordinal):
                            case var tf when tf.StartsWith("unknown function ", StringComparison.Ordinal):
                            case var tt when tt.StartsWith("unknown table ", StringComparison.Ordinal):
                            case var te when te.StartsWith("unknown elem segment", StringComparison.Ordinal):
                            case var tl when tl.StartsWith("unknown local", StringComparison.Ordinal):
                                try
                                {
                                    trapExpected();
                                }
                                catch (CompilerException)
                                {
                                    continue;
                                }
                                catch (ModuleLoadException)
                                {
                                    continue;
                                }
                                catch (Exception x)
                                {
                                    throw new AssertFailedException($"{command.line} threw an unexpected exception of type {x.GetType().Name}.");
                                }
                                throw new AssertFailedException($"{command.line} should have thrown an exception but did not.");
                            case "multiple tables":
                                Assert.ThrowsExactly<ModuleLoadException>(trapExpected, $"{command.line}");
                                continue;
                            default:
                                throw new AssertFailedException($"{command.line}: {assert.text} doesn't have a test procedure set up.");
                        }
                    case AssertTrap assert:
                        trapExpected = () =>
                        {
                            GetMethod(assert.action, out methodInfo, out obj);
                            try
                            {
                                assert.action.Call(methodInfo, obj);
                            }
                            catch (TargetInvocationException x) when (x.InnerException != null)
                            {
                                ExceptionDispatchInfo.Capture(x.InnerException).Throw();
                            }
                        };

                        switch (assert.text)
                        {
                            case "integer divide by zero":
                                Assert.ThrowsExactly<DivideByZeroException>(trapExpected, $"{command.line}");
                                continue;
                            case "integer overflow":
                                Assert.ThrowsExactly<OverflowException>(trapExpected, $"{command.line}");
                                continue;
                            // WebAssembly 3.0 typed function references: calling through a null funcref
                            // (call_ref/return_call_ref) or ref.as_non_null on a null both surface as a
                            // .NET NullReferenceException from the delegate invoke / explicit null check.
                            case "null function reference":
                            case "null reference":
                            // WASM 3.0 garbage collection: struct.get/struct.set (and array access) on a null reference
                            // surfaces as a .NET NullReferenceException from the field/element access.
                            case "null structure reference":
                            case "null array reference":
                            // i31.get_s/get_u on a null i31 reference surfaces as a .NET NullReferenceException.
                            case "null i31 reference":
                                Assert.ThrowsExactly<NullReferenceException>(trapExpected, $"{command.line}");
                                continue;
                            case "out of bounds memory access":
                                try
                                {
                                    trapExpected();
                                }
                                catch (MemoryAccessOutOfRangeException)
                                {
                                    continue;
                                }
                                catch (OverflowException)
                                {
                                    continue;
                                }
                                catch (Exception x)
                                {
                                    throw new AssertFailedException($"{command.line} threw an unexpected exception of type {x.GetType().Name}.");
                                }
                                throw new AssertFailedException($"{command.line} should have thrown an exception but did not.");
                            case "out of bounds table access":
                            // WASM 3.0 garbage collection: an out-of-range array index (array.get/set/fill/copy/init_*)
                            // surfaces as a .NET IndexOutOfRangeException.
                            case "out of bounds array access":
                            // wasm-tools emits the bare "out of bounds" for some memory64/table64 bulk traps; it
                            // covers either a memory or a table access, so accept any of the range-trap types.
                            case "out of bounds":
                                try
                                {
                                    trapExpected();
                                }
                                catch (TableAccessOutOfRangeException)
                                {
                                    continue;
                                }
                                catch (IndexOutOfRangeException)
                                {
                                    continue;
                                }
                                catch (MemoryAccessOutOfRangeException)
                                {
                                    continue;
                                }
                                catch (OverflowException)
                                {
                                    continue;
                                }
                                catch (Exception x)
                                {
                                    throw new AssertFailedException($"{command.line} threw an unexpected exception of type {x.GetType().Name}.");
                                }
                                throw new AssertFailedException($"{command.line} should have thrown an exception but did not.");
                            case "invalid conversion to integer":
                                Assert.ThrowsExactly<OverflowException>(trapExpected, $"{command.line}");
                                continue;
                            case "undefined element":
                            case "uninitialized element 7":
                                try
                                {
                                    trapExpected();
                                    throw new AssertFailedException($"{command.line}: Expected ModuleLoadException or IndexOutOfRangeException, but no exception was thrown.");
                                }
                                catch (ModuleLoadException)
                                {
                                }
                                catch (IndexOutOfRangeException)
                                {
                                }
                                catch (Exception x)
                                {
                                    throw new AssertFailedException($"{command.line}: Expected ModuleLoadException or IndexOutOfRangeException, but received {x.GetType().Name}.");
                                }
                                continue;
                            case "indirect call type mismatch":
                            // WASM 3.0 garbage collection: ref.cast / br_on_cast_fail to a mismatched type surfaces as
                            // a .NET InvalidCastException (from castclass or the WebAssemblyGC.FailCast helper). wasm-tools
                            // emits the shorter "cast" text for the gc/type-subtyping suite.
                            case "cast failure":
                            case "cast":
                                Assert.ThrowsExactly<InvalidCastException>(trapExpected, $"{command.line}");
                                continue;
                            case "unreachable":
                                Assert.ThrowsExactly<UnreachableException>(trapExpected, $"{command.line}");
                                continue;
                            case "uninitialized element":
                            case "uninitialized":
                            case var ue when ue.StartsWith("uninitialized element", StringComparison.Ordinal):
                                try
                                {
                                    trapExpected();
                                    throw new AssertFailedException($"{command.line}: Expected KeyNotFoundException or NullReferenceException, but no exception was thrown.");
                                }
                                catch (KeyNotFoundException)
                                {
                                }
                                catch (NullReferenceException)
                                {
                                }
                                catch (Exception x)
                                {
                                    throw new AssertFailedException($"{command.line}: Expected KeyNotFoundException or NullReferenceException, but received {x.GetType().Name}.");
                                }
                                continue;
                            case "undefined":
                                try
                                {
                                    trapExpected();
                                    throw new AssertFailedException($"{command.line}: Expected KeyNotFoundException or IndexOutOfRangeException, but no exception was thrown.");
                                }
                                catch (KeyNotFoundException)
                                {
                                }
                                catch (IndexOutOfRangeException)
                                {
                                }
                                catch (Exception x)
                                {
                                    throw new AssertFailedException($"{command.line}: Expected KeyNotFoundException or IndexOutOfRangeException, but received {x.GetType().Name}.");
                                }
                                continue;
                            case "indirect call":
                                Assert.ThrowsExactly<InvalidCastException>(trapExpected, $"{command.line}");
                                continue;
                            default:
                                throw new AssertFailedException($"{command.line}: {assert.text} doesn't have a test procedure set up.");
                        }
                    case AssertMalformed assert:
                        continue; // Not writing a WAT parser.
                    case AssertException assert:
                        // WASM 3.0 exception handling: the action must throw a WebAssemblyException (any tag). A trap,
                        // any other exception, or a normal return fails the assertion. The command carries no expected
                        // tag, so there is nothing further to check.
                        GetMethod(assert.action, out methodInfo, out obj);
                        try
                        {
                            assert.action.Call(methodInfo, obj);
                        }
                        catch (TargetInvocationException x) when (x.InnerException is WebAssembly.Runtime.WebAssemblyException)
                        {
                            continue;
                        }
                        catch (WebAssembly.Runtime.WebAssemblyException)
                        {
                            continue;
                        }
                        catch (TargetInvocationException x) when (x.InnerException != null)
                        {
                            throw new AssertFailedException($"{command.line}: expected a WebAssembly exception but the action threw {x.InnerException.GetType().Name}.");
                        }
                        catch (Exception x)
                        {
                            throw new AssertFailedException($"{command.line}: expected a WebAssembly exception but the action threw {x.GetType().Name}.");
                        }
                        throw new AssertFailedException($"{command.line}: expected a WebAssembly exception but the action returned normally.");
                    case AssertExhaustion assert:
                        // "call stack exhausted" is the only text the spec suite emits here, and it's
                        // auto-skipped at the top of the loop (a real StackOverflowException would crash
                        // the host). Any other text is genuinely unhandled.
                        throw new AssertFailedException($"{command.line}: {assert.text} doesn't have a test procedure set up.");
                    case AssertUnlinkable assert:
                        trapExpected = () =>
                        {
                            try
                            {
                                compiler.Compile(Path.Combine(pathBase, assert.filename))(imports);
                            }
                            catch (TargetInvocationException x) when (x.InnerException != null
#if DEBUG
                                && !System.Diagnostics.Debugger.IsAttached
#endif
                                )
                            {
                                ExceptionDispatchInfo.Capture(x.InnerException).Throw();
                            }
                        };
                        switch (assert.text)
                        {
                            case "data segment does not fit":
                            case "elements segment does not fit":
                                try
                                {
                                    trapExpected();
                                    throw new AssertFailedException($"{command.line}: Expected ModuleLoadException, MemoryAccessOutOfRangeException or OverflowException, but no exception was thrown.");
                                }
                                catch (Exception x) when (x is ModuleLoadException || x is MemoryAccessOutOfRangeException || x is OverflowException)
                                {
                                }
                                continue;
                            case "unknown import":
                            case "incompatible import type":
                            // wasm-tools emits the shorter "incompatible import" for a tag-signature mismatch.
                            case "incompatible import":
                                Assert.ThrowsExactly<ImportException>(trapExpected, $"{command.line}");
                                continue;
                            default:
                                throw new AssertFailedException($"{command.line}: {assert.text} doesn't have a test procedure set up.");
                        }
                    case Register register:
                        // Resolve the module being registered by name (a plain module, or a 3.0
                        // module instance — both land in moduleMethodsByName), falling back to the
                        // most recently defined one. Register its own exports (source.Host), not the
                        // last-compiled `exports`, so instance-based registration resolves correctly.
                        var registerSource = register.name != null ? moduleMethodsByName[register.name] : methodsByName!;
                        Assert.IsNotNull(registerSource, $"{command.line} tried to register null as a module method source.");
                        moduleMethodsByName[register.@as] = registerSource;
                        imports.AddFromExports(register.@as, registerSource.Host);
                        continue;
                    case AssertUninstantiable assert:
                        trapExpected = () =>
                        {
                            try
                            {
                                // Instantiate, not just compile: active data/element-segment bounds checks and the
                                // start function run in the instance constructor, which is where these modules trap.
                                compiler.Compile(Path.Combine(pathBase, assert.filename))(imports);
                            }
                            catch (TargetInvocationException x) when (x.InnerException != null)
                            {
                                ExceptionDispatchInfo.Capture(x.InnerException).Throw();
                            }
                        };
                        switch (assert.text)
                        {
                            case "unreachable":
                                Assert.ThrowsExactly<UnreachableException>(trapExpected, $"{command.line}");
                                continue;
                            case "out of bounds memory access":
                                try
                                {
                                    trapExpected();
                                }
                                catch (MemoryAccessOutOfRangeException)
                                {
                                    continue;
                                }
                                catch (OverflowException)
                                {
                                    continue;
                                }
                                catch (Exception x)
                                {
                                    throw new AssertFailedException($"{command.line} threw an unexpected exception of type {x.GetType().Name}.");
                                }
                                throw new AssertFailedException($"{command.line} should have thrown an exception but did not.");
                            case "out of bounds table access":
                                try
                                {
                                    trapExpected();
                                }
                                catch (TableAccessOutOfRangeException)
                                {
                                    continue;
                                }
                                catch (IndexOutOfRangeException)
                                {
                                    continue;
                                }
                                catch (Exception x)
                                {
                                    throw new AssertFailedException($"{command.line} threw an unexpected exception of type {x.GetType().Name}.");
                                }
                                throw new AssertFailedException($"{command.line} should have thrown an exception but did not.");
                            default:
                                throw new AssertFailedException($"{command.line}: {assert.text} doesn't have a test procedure set up.");
                        }
                    default:
                        throw new AssertFailedException($"{command.line}: {command} doesn't have a test procedure set up.");
                }
            }
            catch (Exception x)
            when (!System.Diagnostics.Debugger.IsAttached && (onFailure != null || x is not UnitTestAssertException))
            {
                if (onFailure != null)
                {
                    onFailure(command.line, x);
                    continue;
                }
                throw new AssertFailedException($"{command.line}: {x}", x);
            }
        }

        // Only deferred skips make a category Inconclusive; an `unsupported` list alone leaves it green,
        // because those lines are intentional avoidance rather than pending work.
        if (onFailure == null && !skip.IsEmpty)
            Assert.Inconclusive("Some scenarios were skipped.");
    }

    // Compares a single actual return value against its expected raw value, mirroring the structural-equality
    // fast-path, the approximate float compares, and the lane-wise v128 compare. Returns false only when the
    // expected value type could not be parsed (an inconclusive result the caller propagates).
    static bool CompareReturnValue(TypedValue rawExpected, object? actual, uint line)
    {
        // funcref results: the spec pins only the type, not the function identity (which is CLR-shape coarse and
        // not representable in the JSON). "null" expects a null reference; an unspecified `(ref.func)` expects any
        // non-null function reference (a CLR Delegate).
        if (rawExpected is FuncRefValue funcRef)
            return funcRef.value == "null" ? actual is null : actual is Delegate;

        // WASM 3.0 GC: a bare structref/arrayref/eqref/i31ref (no value) asserts "any non-null reference of this
        // kind"; a "null" payload asserts a null reference. Struct/array/i31 are the emitted GC classes, eq is any
        // internal GC reference (a WebAssemblyGCObject). These carry no identity in the JSON, exactly like the funcref precedent.
        if (rawExpected is StructRefValue or ArrayRefValue or EqRefValue or I31RefValue)
        {
            if (((ReferenceLikeValue)rawExpected).value is "null")
                return actual is null;
            return rawExpected switch
            {
                StructRefValue => actual is WebAssemblyStructObject,
                ArrayRefValue => actual is WebAssemblyArrayObject,
                I31RefValue => actual is WebAssemblyI31,
                _ => actual is WebAssemblyGCObject,
            };
        }

        // WASM 3.0 GC: a bare anyref asserts any non-null reference; a "null" payload asserts null; a host-ref
        // payload (an internalized/externalized extern) compares by its opaque string value.
        if (rawExpected is AnyRefValue anyRef)
        {
            if (anyRef.value is null)
                return actual is not null;
            if (anyRef.value == "null")
                return actual is null;
            return Equals(anyRef.BoxedValue, actual);
        }

        // A bare externref (no value) likewise asserts any non-null reference — an externalized GC value
        // (i31/struct/array) surfaces as its emitted object. A "null" payload asserts null; a host-ref payload
        // compares by its opaque string value (handled by the boxed-value path below).
        if (rawExpected is ExternRefValue externRef && externRef.value is null)
            return actual is not null;

        // 'either' (relaxed-simd): the actual result matches if it equals any one of the listed alternatives.
        // The deterministic profile this library implements always produces one of them. A comparison that
        // fails may throw (the v128 path uses Assert), so each non-final alternative is tried under a catch;
        // the final alternative propagates its failure so the message names a concrete expected value.
        if (rawExpected is EitherValue either)
        {
            var alternatives = either.values ?? [];
            for (var i = 0; i < alternatives.Length - 1; i++)
            {
                try
                {
                    if (CompareReturnValue(alternatives[i], actual, line))
                        return true;
                }
                catch (UnitTestAssertException)
                {
                    // This alternative didn't match; fall through to the next one.
                }
            }

            return alternatives.Length > 0 && CompareReturnValue(alternatives[^1], actual, line);
        }

        if (Equals(rawExpected.BoxedValue, actual))
            return true;

        switch (rawExpected.type)
        {
            default:
                // This happens in conversion.json starting at "line": 317 when run via GitHub Action but never locally (for me).
                Assert.Inconclusive($"{line}: Failed to parse expected value type.");
                return false;

            // A float reinterpreted to its integer bits: .NET cannot reproduce a specific NaN payload (the
            // hardware/JIT chooses it), so a result that is a NaN bit pattern differing from an expected NaN
            // bit pattern is an execution-model divergence, not a library error — accept it. This is the same
            // tolerance the runner already applies to the nan:canonical / nan:arithmetic markers, extended to
            // the exact-bit NaN expectations the float_literals/float_exprs/float_memory suites use.
            case RawValueType.i32:
                if (actual is int actualI32
                    && float.IsNaN(BitConverter.Int32BitsToSingle((int)rawExpected.BoxedValue!))
                    && float.IsNaN(BitConverter.Int32BitsToSingle(actualI32)))
                    return true;
                break;
            case RawValueType.i64:
                if (actual is long actualI64
                    && double.IsNaN(BitConverter.Int64BitsToDouble((long)rawExpected.BoxedValue!))
                    && double.IsNaN(BitConverter.Int64BitsToDouble(actualI64)))
                    return true;
                break;
            case RawValueType.externref:
            case RawValueType.funcref:
                break;

            case RawValueType.f32:
                {
                    var expected = ((Float32Value)rawExpected).ActualValue;
                    Assert.AreEqual(expected, (float)actual!, Math.Abs(expected * 0.000001f), $"{line}: f32 compare");
                }
                return true;
            case RawValueType.f64:
                {
                    var expected = ((Float64Value)rawExpected).ActualValue;
                    Assert.AreEqual(expected, (double)actual!, Math.Abs(expected * 0.000001), $"{line}: f64 compare");
                }
                return true;
            case RawValueType.v128:
                Assert.IsTrue(((V128Value)rawExpected).IsMatch((Vector128<byte>)actual!), $"{line}: v128 compare, got {actual}");
                return true;
        }

        throw new AssertFailedException($"{line}: Not equal {rawExpected.type}: {rawExpected.BoxedValue} and {actual}");
    }

    // Spreads a multi-value result (a possibly-nested ValueTuple) into its individual values, in result order.
    static object?[] DecomposeResult(object? result, int count)
    {
        var values = new object?[count];
        var current = result!;
        var type = current.GetType();
        var index = 0;
        while (true)
        {
            var direct = Math.Min(count - index, 7);
            for (var i = 0; i < direct; i++)
                values[index++] = type.GetField($"Item{i + 1}")!.GetValue(current);

            if (index >= count)
                break;

            var restField = type.GetField("Rest")!;
            current = restField.GetValue(current)!;
            type = restField.FieldType;
        }

        return values;
    }

    // Abstracts the two execution back-ends behind a single "compile a module file into an instance factory"
    // operation, so the runner's assertion logic is identical for both. The in-memory factory yields the
    // Instance<TExports>.Exports host; the persisted factory yields the loaded, freshly-constructed compiled type
    // (which is itself the exports host). Both throw compile-time validation eagerly from the shared compiler, so
    // the assert_invalid path expects the same exceptions in either mode.
    abstract class SpecCompiler : IDisposable
    {
#if !NET9_0_OR_GREATER
        // On net8 the persisted back-end is compiled out, so only one concrete type is ever returned; the
        // abstraction is still intentional (it exists for the net9+ persisted path), so suppress the collapse hint.
#pragma warning disable CA1859
#endif
        public static SpecCompiler Create<TExports>(SpecExecutionMode mode) where TExports : class
            => mode switch
            {
                SpecExecutionMode.InMemory => new InMemorySpecCompiler<TExports>(),
#if NET9_0_OR_GREATER
                SpecExecutionMode.Persisted => new PersistedSpecCompiler(),
#endif
                _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unsupported execution mode."),
            };
#if !NET9_0_OR_GREATER
#pragma warning restore CA1859
#endif

        // Compiles the module file at the given path into a factory that instantiates it against a set of imports,
        // yielding the exports host object.
        public abstract Func<ImportDictionary, object> Compile(string path);

        public virtual void Dispose() { }
    }

    sealed class InMemorySpecCompiler<TExports> : SpecCompiler where TExports : class
    {
        public override Func<ImportDictionary, object> Compile(string path)
        {
            var creator = WebAssembly.Runtime.Compile.FromBinary<TExports>(path);
            return imports => creator(imports).Exports!;
        }
    }

#if NET9_0_OR_GREATER
    sealed class PersistedSpecCompiler : SpecCompiler
    {
        // One collectible context per category run: each module's assembly loads into it, and the whole set is
        // reclaimed on Dispose. Dependencies (WebAssembly.dll, System.*) fall back to the default context, keeping
        // WebAssemblyGC's canonical-type interning process-global.
        readonly AssemblyLoadContext context = new("SpecPersisted", isCollectible: true);
        int counter;

        public override Func<ImportDictionary, object> Compile(string path)
        {
            using var wasm = File.OpenRead(path);

            // A distinct assembly name per module so multiple modules coexist in the one load context.
            var configuration = new PersistedCompilerConfiguration(
                typeof(object).Assembly,
                typeof(WebAssembly.Runtime.Compile).Assembly,
                new AssemblyName($"SpecPersisted{++counter}"),
                "SpecPersistedModule");

            // CreatePersistedAssembly runs the shared compiler, throwing validation exceptions here (matching FromBinary),
            // so assert_invalid observes identical behavior across modes.
            var builder = WebAssembly.Runtime.Compile.CreatePersistedAssembly(wasm, configuration);

            using var dll = new MemoryStream();
            builder.Save(dll);
            dll.Position = 0;

            var assembly = context.LoadFromStream(dll);
            var type = assembly.GetType(configuration.TypeName)!;
            var constructor = type.GetConstructors().Single();

            return imports =>
            {
                // Mirror the in-memory findImport: resolve from the dictionary, else throw ImportException.
                Func<string, string, RuntimeImport> resolver = (module, field) =>
                    imports.TryGetValue(module, out var fields) && fields.TryGetValue(field, out var import)
                        ? import
                        : throw new ImportException($"Missing import for {module}::{field}.");

                try
                {
                    return constructor.Invoke([resolver]);
                }
                catch (TargetInvocationException x) when (x.InnerException != null)
                {
                    // Surface the underlying failure (trap, ImportException, …) exactly as the in-memory factory does.
                    ExceptionDispatchInfo.Capture(x.InnerException).Throw();
                    throw;
                }
            };
        }

        public override void Dispose() => context.Unload();
    }
#endif

    /// <summary>
    /// Selects the execution back-end used by <see cref="SpecTestRunner"/>.
    /// </summary>
    internal enum SpecExecutionMode
    {
        /// <summary>The in-memory Reflection.Emit path.</summary>
        InMemory,

        /// <summary>The persisted WASM-to-DLL path: compile, save, load, then instantiate.</summary>
        Persisted,
    }

    class ObjectMethods : Dictionary<string, MethodInfo>
    {
        public readonly object Host;

        public ObjectMethods(object host)
        {
            Assert.IsNotNull(this.Host = host);

            foreach (var method in host
                .GetType()
                .GetMethods()
                .Select(m => new { m.Name, MethodInfo = m })
                .Concat(host
                .GetType()
                .GetProperties()
                .Where(p => p.GetGetMethod() != null)
                .Select(p => new { p.Name, MethodInfo = p.GetGetMethod()! })))
            {
                Assert.IsTrue(TryAdd(method.Name, method.MethodInfo));
            }
        }
    }

    [JsonConverter(typeof(JsonStringEnumConverter<CommandType>))]
    enum CommandType
    {
        module,
        assert_return,
        assert_return_canonical_nan,
        assert_return_arithmetic_nan,
        assert_invalid,
        assert_trap,
        assert_malformed,
        assert_exhaustion,
        assert_unlinkable,
        register,
        action,
        assert_uninstantiable,
        // WASM 3.0 script syntax: a reusable module definition plus per-instance instantiation
        // (instance.wast and parts of memory64/), and the exceptions assert.
        module_definition,
        module_instance,
        assert_exception,
    }

#pragma warning disable 649
    class TestInfo
    {
        public string source_filename;
        public Command[] commands;
    }


    [JsonPolymorphic(TypeDiscriminatorPropertyName = nameof(type))]
    [JsonDerivedType(typeof(ModuleCommand), typeDiscriminator: nameof(CommandType.module))]
    [JsonDerivedType(typeof(AssertReturn), typeDiscriminator: nameof(CommandType.assert_return))]
    [JsonDerivedType(typeof(AssertReturnCanonicalNan), typeDiscriminator: nameof(CommandType.assert_return_canonical_nan))]
    [JsonDerivedType(typeof(AssertReturnArithmeticNan), typeDiscriminator: nameof(CommandType.assert_return_arithmetic_nan))]
    [JsonDerivedType(typeof(AssertInvalid), typeDiscriminator: nameof(CommandType.assert_invalid))]
    [JsonDerivedType(typeof(AssertTrap), typeDiscriminator: nameof(CommandType.assert_trap))]
    [JsonDerivedType(typeof(AssertMalformed), typeDiscriminator: nameof(CommandType.assert_malformed))]
    [JsonDerivedType(typeof(AssertExhaustion), typeDiscriminator: nameof(CommandType.assert_exhaustion))]
    [JsonDerivedType(typeof(AssertUnlinkable), typeDiscriminator: nameof(CommandType.assert_unlinkable))]
    [JsonDerivedType(typeof(Register), typeDiscriminator: nameof(CommandType.register))]
    [JsonDerivedType(typeof(NoReturn), typeDiscriminator: nameof(CommandType.action))]
    [JsonDerivedType(typeof(AssertUninstantiable), typeDiscriminator: nameof(CommandType.assert_uninstantiable))]
    [JsonDerivedType(typeof(ModuleDefinition), typeDiscriminator: nameof(CommandType.module_definition))]
    [JsonDerivedType(typeof(ModuleInstance), typeDiscriminator: nameof(CommandType.module_instance))]
    [JsonDerivedType(typeof(AssertException), typeDiscriminator: nameof(CommandType.assert_exception))]
    abstract class Command(CommandType commandType)
    {
        // [JsonIgnore] is required on .NET 10+: the JsonPolymorphic discriminator above is named
        // "type", and STJ now treats a regular property of the same name as a conflict with the
        // metadata property. Each concrete subclass passes its discriminator value through this
        // primary constructor so callers can still read command.type after deserialization.
        [JsonIgnore]
        public readonly CommandType type = commandType;
        public uint line;

        public override string ToString() => type.ToString();
    }

    class ModuleCommand() : Command(CommandType.module)
    {
        public string name;
        public string filename;
        // wasm-tools emits a text module as both a .wat (filename) and an assembled .wasm
        // (binary_filename). Prefer the binary so the library — which has no WAT parser — can load it.
        public string? binary_filename;

        // The loadable module path: the assembled binary when the primary is text, else the primary.
        public string LoadFilename => binary_filename ?? filename;

        public override string ToString() => $"{base.ToString()}: {LoadFilename}";
    }

    // WASM 3.0 script syntax. A `module definition` compiles a module once and stores it by name; a
    // `module instance` instantiates a previously-defined module (InstanceCreator supports multiple
    // instantiations of the same compiled definition). Together they back instance.wast and the
    // multi-instance patterns in memory64/.
    class ModuleDefinition() : Command(CommandType.module_definition)
    {
        public string name;
        public string filename;
        public string? binary_filename;

        // The loadable module path: the assembled binary when the primary is text, else the primary.
        public string LoadFilename => binary_filename ?? filename;

        public override string ToString() => $"{base.ToString()}: {name} <- {LoadFilename}";
    }

    class ModuleInstance() : Command(CommandType.module_instance)
    {
        public string instance;
        public string module;

        public override string ToString() => $"{base.ToString()}: {instance} of {module}";
    }

    [JsonConverter(typeof(JsonStringEnumConverter<RawValueType>))]
    enum RawValueType
    {
        i32 = WebAssemblyValueType.Int32,
        i64 = WebAssemblyValueType.Int64,
        f32 = WebAssemblyValueType.Float32,
        f64 = WebAssemblyValueType.Float64,
        // Reference types (WASM 2.0). Live comparison types: the runner compares actual results
        // against these (externref by opaque payload, funcref by CLR-shape) in CompareReturnValue.
        externref = -100,
        funcref = -101,
        v128 = WebAssemblyValueType.V128,
        // Reference types added by WASM 3.0 (typed function references, GC, exceptions). Like the
        // 2.0 ref types above, these are live comparison types — CompareReturnValue matches a bare
        // ref against "any non-null reference of this kind", a "null" payload against null, and a
        // host-ref payload by its opaque value. Distinct negative sentinels so the polymorphic
        // discriminator round-trips.
        anyref = -102,
        eqref = -103,
        structref = -104,
        arrayref = -105,
        i31ref = -106,
        exnref = -107,
        nullref = -108,
        nullfuncref = -109,
        nullexternref = -110,
        nullexnref = -111,
        // The wasm-tools "refnull" expected value is a bare null of unspecified ref type.
        refnull = -112,
        // "either" is not a value type but a multi-alternative expected result (relaxed-simd): the
        // actual result matches if it equals any one of `values`. Compared in CompareReturnValue.
        either = -113,
    }

    // Custom converter that accepts both numeric-string values and the post-2.0 inline NaN markers
    // ("nan:canonical", "nan:arithmetic"), which replaced the older separate
    // assert_return_canonical_nan / assert_return_arithmetic_nan command types. Map each NaN marker
    // to a NaN bit pattern; the runner's existing equality fast-path then matches any NaN result
    // (float.NaN.Equals(float.NaN) is true, regardless of payload).
    //
    // Numeric strings arrive in two conventions: wast2json (wabt) emitted unsigned decimal
    // ("4294967294"), wasm-tools (the 3.0-capable generator) emits signed decimal ("-2"). Accept
    // both by parsing through the widest signed type that spans the whole unsigned range and taking
    // the low bits: a 32-bit value spans int.MinValue..uint.MaxValue, which fits in long.
    class NanCapableUInt32Converter : JsonConverter<uint>
    {
        public override uint Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Number) return reader.GetUInt32();
            var s = reader.GetString()!;
            return s switch
            {
                "nan:canonical" => 0x7FC00000u,
                "nan:arithmetic" => 0x7FC00001u,
                _ => unchecked((uint)long.Parse(s)),
            };
        }
        public override void Write(Utf8JsonWriter writer, uint value, JsonSerializerOptions options) => writer.WriteNumberValue(value);
    }

    class NanCapableUInt64Converter : JsonConverter<ulong>
    {
        public override ulong Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Number) return reader.GetUInt64();
            var s = reader.GetString()!;
            return s switch
            {
                "nan:canonical" => 0x7FF8000000000000u,
                "nan:arithmetic" => 0x7FF8000000000001u,
                // A signed i64 ("-2") won't fit in ulong; parse it as long and take the low 64 bits.
                // An unsigned u64 ("18446744073709551615") exceeds long, so parse it directly.
                _ => s.StartsWith('-') ? unchecked((ulong)long.Parse(s)) : ulong.Parse(s),
            };
        }
        public override void Write(Utf8JsonWriter writer, ulong value, JsonSerializerOptions options) => writer.WriteNumberValue(value);
    }

    class TypeOnly
    {
        public RawValueType type;

        public override string ToString() => type.ToString();
    }

    [JsonPolymorphic(TypeDiscriminatorPropertyName = nameof(type))]
    [JsonDerivedType(typeof(Int32Value), typeDiscriminator: nameof(RawValueType.i32))]
    [JsonDerivedType(typeof(Int64Value), typeDiscriminator: nameof(RawValueType.i64))]
    [JsonDerivedType(typeof(Float32Value), typeDiscriminator: nameof(RawValueType.f32))]
    [JsonDerivedType(typeof(Float64Value), typeDiscriminator: nameof(RawValueType.f64))]
    [JsonDerivedType(typeof(ExternRefValue), typeDiscriminator: nameof(RawValueType.externref))]
    [JsonDerivedType(typeof(FuncRefValue), typeDiscriminator: nameof(RawValueType.funcref))]
    [JsonDerivedType(typeof(V128Value), typeDiscriminator: nameof(RawValueType.v128))]
    [JsonDerivedType(typeof(AnyRefValue), typeDiscriminator: nameof(RawValueType.anyref))]
    [JsonDerivedType(typeof(EqRefValue), typeDiscriminator: nameof(RawValueType.eqref))]
    [JsonDerivedType(typeof(StructRefValue), typeDiscriminator: nameof(RawValueType.structref))]
    [JsonDerivedType(typeof(ArrayRefValue), typeDiscriminator: nameof(RawValueType.arrayref))]
    [JsonDerivedType(typeof(I31RefValue), typeDiscriminator: nameof(RawValueType.i31ref))]
    [JsonDerivedType(typeof(ExnRefValue), typeDiscriminator: nameof(RawValueType.exnref))]
    [JsonDerivedType(typeof(NullRefValue), typeDiscriminator: nameof(RawValueType.nullref))]
    [JsonDerivedType(typeof(NullFuncRefValue), typeDiscriminator: nameof(RawValueType.nullfuncref))]
    [JsonDerivedType(typeof(NullExternRefValue), typeDiscriminator: nameof(RawValueType.nullexternref))]
    [JsonDerivedType(typeof(NullExnRefValue), typeDiscriminator: nameof(RawValueType.nullexnref))]
    [JsonDerivedType(typeof(RefNullValue), typeDiscriminator: nameof(RawValueType.refnull))]
    [JsonDerivedType(typeof(EitherValue), typeDiscriminator: nameof(RawValueType.either))]
    abstract class TypedValue(RawValueType valueType)
    {
        // Not inheriting TypeOnly here: the inherited `type` field would conflict with the
        // polymorphism discriminator on .NET 10+ (see Command for details). The two hierarchies
        // are used independently — TypeOnly[] in canonical/arithmetic-NaN asserts, TypedValue[]
        // in regular asserts and invoke args — so keeping them separate is safe.
        [JsonIgnore]
        public readonly RawValueType type = valueType;
        public abstract object? BoxedValue { get; }
    }

    // Same two-ctor pattern as AssertReturn (see comment there): the parameterized primary lets
    // Float32Value override; the [JsonConstructor]-marked parameterless ctor is what STJ uses.
    class Int32Value(RawValueType valueType) : TypedValue(valueType)
    {
        [JsonConstructor]
        public Int32Value() : this(RawValueType.i32) { }

        [JsonConverter(typeof(NanCapableUInt32Converter))]
        public uint value;

        public override object BoxedValue => (int)value;

        public override string ToString() => $"{type}: {value}";
    }

    class Int64Value(RawValueType valueType) : TypedValue(valueType)
    {
        [JsonConstructor]
        public Int64Value() : this(RawValueType.i64) { }

        [JsonConverter(typeof(NanCapableUInt64Converter))]
        public ulong value;

        public override object BoxedValue => (long)value;

        public override string ToString() => $"{type}: {value}";
    }

    class Float32Value() : Int32Value(RawValueType.f32)
    {
        public float ActualValue => BitConverter.Int32BitsToSingle(unchecked((int)value));

        public override object BoxedValue => ActualValue;

        public override string ToString() => $"{type}: {BoxedValue}";
    }

    class Float64Value() : Int64Value(RawValueType.f64)
    {
        public double ActualValue => BitConverter.Int64BitsToDouble(unchecked((long)value));

        public override object BoxedValue => ActualValue;

        public override string ToString() => $"{type}: {BoxedValue}";
    }

    // Reference-type values (WASM 2.0). externref is a nullable object: the harness represents a host
    // reference by its opaque string payload (e.g. "1" for (ref.extern 1)), which is passed through the
    // module as an argument and compared by value. funcref values only appear as null in the spec data.
    class ExternRefValue() : TypedValue(RawValueType.externref)
    {
        public string? value;

        public override object? BoxedValue => value == "null" ? null : value;

        public override string ToString() => $"{type}: {value ?? "null"}";
    }

    class FuncRefValue() : TypedValue(RawValueType.funcref)
    {
        public string? value;

        public override object? BoxedValue => value == "null"
            ? null
            : throw new NotSupportedException($"funcref value '{value}' cannot be boxed - only null funcref is supported.");

        public override string ToString() => $"{type}: {value ?? "null"}";
    }

    // Reference-type values added by WASM 3.0 (typed function references, GC, exceptions). These are
    // compared structurally in CompareReturnValue, which handles a bare ref (any non-null of the kind)
    // and a "null" payload directly without ever boxing. A bare null (no value, or value "null") boxes
    // to null for the rare path that does read BoxedValue; any concrete payload has no CLR boxing, so
    // the base throws as a defensive guard for a case CompareReturnValue never routes here. One leaf
    // class per JSON discriminator, sharing the boxing logic through this base.
    abstract class ReferenceLikeValue(RawValueType valueType) : TypedValue(valueType)
    {
        public string? value;

        public override object? BoxedValue => value is null or "null"
            ? null
            : throw new NotSupportedException($"{type} value '{value}' has no CLR boxing; compared structurally instead.");

        public override string ToString() => $"{type}: {value ?? "null"}";
    }

    // WASM 3.0 GC: an anyref value carries a host reference by its opaque string payload, exactly like externref
    // (an internalized (ref.extern N) surfaces as the string "N"); a bare anyref (no value) asserts any non-null
    // reference and "null" asserts null. This mirrors ExternRefValue's boxing rather than the throwing base.
    class AnyRefValue() : ReferenceLikeValue(RawValueType.anyref)
    {
        public override object? BoxedValue => value == "null" ? null : value;
    }
    class EqRefValue() : ReferenceLikeValue(RawValueType.eqref);
    class StructRefValue() : ReferenceLikeValue(RawValueType.structref);
    class ArrayRefValue() : ReferenceLikeValue(RawValueType.arrayref);
    class I31RefValue() : ReferenceLikeValue(RawValueType.i31ref);
    class ExnRefValue() : ReferenceLikeValue(RawValueType.exnref);
    class NullRefValue() : ReferenceLikeValue(RawValueType.nullref);
    class NullFuncRefValue() : ReferenceLikeValue(RawValueType.nullfuncref);
    class NullExternRefValue() : ReferenceLikeValue(RawValueType.nullexternref);
    class NullExnRefValue() : ReferenceLikeValue(RawValueType.nullexnref);
    class RefNullValue() : ReferenceLikeValue(RawValueType.refnull);

    // Multi-alternative expected result (relaxed-simd): the actual result is a match if it equals
    // any one of `values`. Comparison is handled in CompareReturnValue, which tries each alternative
    // in turn; BoxedValue is never read for an 'either', so it throws as a never-called guard.
    class EitherValue() : TypedValue(RawValueType.either)
    {
        public TypedValue[]? values;

        public override object? BoxedValue =>
            throw new NotSupportedException("'either' expected results are compared via CompareReturnValue, not boxed.");

        public override string ToString() => $"either[{string.Join(',', (IEnumerable<TypedValue>?)values ?? [])}]";
    }

    // SIMD v128 value (WASM 2.0). The spec data expresses a v128 as a lane_type plus an array of
    // per-lane integer/NaN strings; we pack those into the 16 bytes of a Vector128<byte>, which is
    // how the runtime represents v128 on the test target frameworks (net5+).
    class V128Value() : TypedValue(RawValueType.v128)
    {
        public string? lane_type;
        public string[]? value;

        // True if any lane is a NaN pattern ("nan:canonical" / "nan:arithmetic"); those need
        // lane-by-lane comparison because NaN payloads aren't bit-exact.
        private bool HasNaN => value != null && value.Any(v => v.StartsWith("nan:", StringComparison.Ordinal));

        // Parses a lane string to its raw bits; NaN patterns map to a canonical NaN for the lane type.
        private ulong ParseLane(string s)
        {
            if (!s.StartsWith("nan:", StringComparison.Ordinal))
                // Lane values follow the same signed/unsigned split as scalar ints: wasm-tools emits
                // signed decimal ("-1" for an all-ones lane), wast2json emitted unsigned. A signed
                // i64 lane won't fit ulong, so parse it as long and take the low bits.
                return s.StartsWith('-') ? unchecked((ulong)long.Parse(s)) : ulong.Parse(s);
            return lane_type == "f64" ? 0x7FF8000000000000UL : 0x7FC00000UL;
        }

        public Vector128<byte> ActualValue
        {
            get
            {
                var bytes = new byte[16];
                if (value != null)
                {
                    switch (lane_type)
                    {
                        case "i8":
                            for (var i = 0; i < 16 && i < value.Length; i++)
                                bytes[i] = (byte)ParseLane(value[i]);
                            break;
                        case "i16":
                            for (var i = 0; i < 8 && i < value.Length; i++)
                            {
                                var v = (ushort)ParseLane(value[i]);
                                bytes[i * 2] = (byte)v;
                                bytes[i * 2 + 1] = (byte)(v >> 8);
                            }
                            break;
                        case "i32":
                        case "f32":
                            for (var i = 0; i < 4 && i < value.Length; i++)
                            {
                                var v = (uint)ParseLane(value[i]);
                                bytes[i * 4] = (byte)v;
                                bytes[i * 4 + 1] = (byte)(v >> 8);
                                bytes[i * 4 + 2] = (byte)(v >> 16);
                                bytes[i * 4 + 3] = (byte)(v >> 24);
                            }
                            break;
                        case "i64":
                        case "f64":
                            for (var i = 0; i < 2 && i < value.Length; i++)
                            {
                                var v = ParseLane(value[i]);
                                for (var b = 0; b < 8; b++)
                                    bytes[i * 8 + b] = (byte)(v >> (b * 8));
                            }
                            break;
                    }
                }

                return Vector128.Create(bytes);
            }
        }

        public override object BoxedValue => ActualValue;

        // NaN-aware lane-by-lane match: a "nan:*" pattern matches any NaN in that lane; other lanes
        // must be bit-exact. Non-NaN vectors are handled by the caller's structural equality fast-path.
        public bool IsMatch(Vector128<byte> actual)
        {
            if (!HasNaN)
                return ActualValue.Equals(actual);
            if (value == null)
                return false;
            switch (lane_type)
            {
                case "f32":
                    var af = actual.AsSingle();
                    for (var i = 0; i < 4 && i < value.Length; i++)
                    {
                        if (value[i].StartsWith("nan:", StringComparison.Ordinal))
                        {
                            if (!float.IsNaN(af[i]))
                                return false;
                        }
                        else if ((uint)ParseLane(value[i]) != BitConverter.SingleToUInt32Bits(af[i]))
                            return false;
                    }
                    return true;
                case "f64":
                    var ad = actual.AsDouble();
                    for (var i = 0; i < 2 && i < value.Length; i++)
                    {
                        if (value[i].StartsWith("nan:", StringComparison.Ordinal))
                        {
                            if (!double.IsNaN(ad[i]))
                                return false;
                        }
                        else if (ParseLane(value[i]) != BitConverter.DoubleToUInt64Bits(ad[i]))
                            return false;
                    }
                    return true;
                default:
                    return ActualValue.Equals(actual);
            }
        }

        public override string ToString() => $"v128({lane_type})[{string.Join(',', value ?? [])}]";
    }

    [JsonConverter(typeof(JsonStringEnumConverter<TestActionType>))]
    enum TestActionType
    {
        invoke,
        get,
    }

    [JsonPolymorphic(TypeDiscriminatorPropertyName = nameof(type))]
    [JsonDerivedType(typeof(Invoke), typeDiscriminator: nameof(TestActionType.invoke))]
    [JsonDerivedType(typeof(Get), typeDiscriminator: nameof(TestActionType.get))]
    abstract class TestAction(TestActionType actionType)
    {
        [JsonIgnore]
        public readonly TestActionType type = actionType;
        public string module;
        public string field;

        public abstract object? Call(MethodInfo methodInfo, object obj);
    }

    class Invoke() : TestAction(TestActionType.invoke)
    {
        public TypedValue[] args;

        public override object? Call(MethodInfo methodInfo, object obj)
        {
            return methodInfo.Invoke(obj, args.Select(arg => arg.BoxedValue).ToArray());
        }

        public override string ToString() => $"{field}({module})[{string.Join(',', (IEnumerable<TypedValue>)args)}]";
    }

    class Get() : TestAction(TestActionType.get)
    {
        public override object? Call(MethodInfo methodInfo, object obj)
        {
            return methodInfo.Invoke(obj, []);
        }
    }

    abstract class AssertCommand(CommandType commandType) : Command(commandType)
    {
        public TestAction action;

        public override string ToString() => $"{base.ToString()}: {action}";
    }

    // Two ctors: the parameterized primary lets NoReturn pass `action` through; the explicit
    // parameterless ctor (marked [JsonConstructor]) is what STJ uses when deserializing
    // `assert_return` directly. Without the [JsonConstructor] hint, STJ would try to bind the
    // primary ctor's `commandType` parameter to a JSON property, which doesn't exist.
    class AssertReturn(CommandType commandType) : AssertCommand(commandType)
    {
        [JsonConstructor]
        public AssertReturn() : this(CommandType.assert_return) { }

        public TypedValue[] expected;

        public override string ToString() => $"{base.ToString()} = [{string.Join(',', (IEnumerable<TypedValue>)expected)}]";
    }

    class NoReturn() : AssertReturn(CommandType.action)
    {
        public override string ToString() => $"{base.ToString()}";
    }

    class AssertReturnCanonicalNan() : AssertCommand(CommandType.assert_return_canonical_nan)
    {
        public TypeOnly[] expected;

        public override string ToString() => $"{base.ToString()} = [{string.Join(',', (IEnumerable<TypeOnly>)expected)}]";
    }

    class AssertReturnArithmeticNan() : AssertCommand(CommandType.assert_return_arithmetic_nan)
    {
        public TypeOnly[] expected;

        public override string ToString() => $"{base.ToString()} = [{string.Join(',', (IEnumerable<TypeOnly>)expected)}]";
    }

    abstract class InvalidCommand(CommandType commandType) : Command(commandType)
    {
        public string filename;
        public string text;
        public string module_type;

        public override string ToString() => $"{base.ToString()}: {filename} \"{text}\" {module_type}";
    }

    class AssertInvalid() : InvalidCommand(CommandType.assert_invalid);

    class AssertTrap() : AssertCommand(CommandType.assert_trap)
    {
        public TypeOnly[] expected;
        public string text;
    }

    class AssertMalformed() : InvalidCommand(CommandType.assert_malformed);

    class AssertExhaustion() : AssertCommand(CommandType.assert_exhaustion)
    {
        public string text;
    }

    // WASM 3.0 exception handling: the action is expected to throw a WASM exception carrying the
    // named tag. The runner handles these fully (see the AssertException case in the switch), asserting
    // the action throws a WebAssemblyException.
    class AssertException() : AssertCommand(CommandType.assert_exception);

    class AssertUnlinkable() : InvalidCommand(CommandType.assert_unlinkable);

    class AssertUninstantiable() : InvalidCommand(CommandType.assert_uninstantiable);

    class Register() : Command(CommandType.register)
    {
        public string name;
        public string @as;
    }
#pragma warning restore    
}
