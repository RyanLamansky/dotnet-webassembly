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

// Effect of this is trusting that the source JSONs are valid.
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

namespace WebAssembly.Runtime;

static partial class SpecTestRunner
{
    public static void Run(string pathBase, string json) => Run<object>(pathBase, json);

    public static void Run(string pathBase, string json, Func<uint, bool>? skip) => Run<object>(pathBase, json, skip);

    public static void Run<TExports>(string pathBase, string json)
        where TExports : class
    {
        Run<TExports>(pathBase, json, null);
    }

    /// <summary>
    /// Runs every command in the JSON file, catching all failures into the returned list rather
    /// than throwing on the first one. Used by <see cref="SpecDiscovery"/> to enumerate the line
    /// numbers that need skip-predicate coverage after a spec test refresh.
    /// </summary>
    public static List<(uint Line, string Message)> Discover(string pathBase, string json)
    {
        var failures = new List<(uint, string)>();
        Run<object>(pathBase, json, skip: null,
            onFailure: (line, x) => failures.Add((line, $"{x.GetType().Name}: {x.Message.Split('\n')[0]}")));
        return failures;
    }

    [JsonSourceGenerationOptions(IncludeFields = true)]
    [JsonSerializable(typeof(TestInfo))]
    private sealed partial class SpecTestSerializerContext : JsonSerializerContext
    {
    }

    public static void Run<TExports>(string pathBase, string json, Func<uint, bool>? skip)
        where TExports : class
    {
        Run<TExports>(pathBase, json, skip, onFailure: null);
    }

    private static void Run<TExports>(string pathBase, string json, Func<uint, bool>? skip, Action<uint, Exception>? onFailure)
        where TExports : class
    {
        TestInfo testInfo;
        using (var reader = File.OpenRead(Path.Combine(pathBase, json)))
        {
            testInfo = JsonSerializer.Deserialize(reader, SpecTestSerializerContext.Default.TestInfo)!;
        }

        ObjectMethods? methodsByName = null;
        var moduleMethodsByName = new Dictionary<string, ObjectMethods>();

        // From https://github.com/WebAssembly/spec/blob/master/interpreter/host/spectest.ml
        var imports = new ImportDictionary
        {
            { "spectest", "print", new FunctionImport((Action)(() => { })) },
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
            { "spectest", "memory", new MemoryImport(() => new UnmanagedMemory(1, 2)) }, // Memory.alloc (MemoryType {min = 1l; max = Some 2l})
        };

        var registrationCandidates = new ImportDictionary();

        Action trapExpected;
        object? result;
        object obj;
        MethodInfo methodInfo;
        TExports? exports = null;
        foreach (var command in testInfo.commands)
        {
            if (skip != null && skip(command.line))
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
                        var path = Path.Combine(pathBase, module.filename);
                        var parsed = Module.ReadFromBinary(path); // Ensure the module parser can read it.
                        Assert.IsNotNull(parsed);
                        methodsByName = new ObjectMethods(exports = Compile.FromBinary<TExports>(path)(imports).Exports);
                        if (module.name != null)
                            moduleMethodsByName[module.name] = methodsByName;
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
                                Compile.FromBinary<TExports>(Path.Combine(pathBase, assert.filename));
                            }
                            catch (TargetInvocationException x) when (x.InnerException != null)
                            {
                                ExceptionDispatchInfo.Capture(x.InnerException).Throw();
                            }
                        };
                        switch (assert.text)
                        {
                            case "type mismatch":
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
                            case "global is immutable":
                                Assert.ThrowsException<CompilerException>(trapExpected, $"{command.line}");
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
                            case "unknown label":
                            case "unknown type":
                            case "unknown data segment":
                            case "unknown data segment 1":
                            case "start function":
                                Assert.ThrowsException<ModuleLoadException>(trapExpected, $"{command.line}");
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
                                Assert.ThrowsException<ModuleLoadException>(trapExpected, $"{command.line}");
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
                                Assert.ThrowsException<DivideByZeroException>(trapExpected, $"{command.line}");
                                continue;
                            case "integer overflow":
                                Assert.ThrowsException<OverflowException>(trapExpected, $"{command.line}");
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
                                catch (MemoryAccessOutOfRangeException)
                                {
                                    continue;
                                }
                                catch (Exception x)
                                {
                                    throw new AssertFailedException($"{command.line} threw an unexpected exception of type {x.GetType().Name}.");
                                }
                                throw new AssertFailedException($"{command.line} should have thrown an exception but did not.");
                            case "invalid conversion to integer":
                                Assert.ThrowsException<OverflowException>(trapExpected, $"{command.line}");
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
                                Assert.ThrowsException<InvalidCastException>(trapExpected, $"{command.line}");
                                continue;
                            case "unreachable":
                                Assert.ThrowsException<UnreachableException>(trapExpected, $"{command.line}");
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
                                Assert.ThrowsException<InvalidCastException>(trapExpected, $"{command.line}");
                                continue;
                            default:
                                throw new AssertFailedException($"{command.line}: {assert.text} doesn't have a test procedure set up.");
                        }
                    case AssertMalformed assert:
                        continue; // Not writing a WAT parser.
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
                                Compile.FromBinary<TExports>(Path.Combine(pathBase, assert.filename))(imports);
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
                                Assert.ThrowsException<ImportException>(trapExpected, $"{command.line}");
                                continue;
                            default:
                                throw new AssertFailedException($"{command.line}: {assert.text} doesn't have a test procedure set up.");
                        }
                    case Register register:
                        Assert.IsNotNull(
                            moduleMethodsByName[register.@as] = register.name != null ? moduleMethodsByName[register.name] : methodsByName!,
                            $"{command.line} tried to register null as a module method source.");
                        Assert.IsNotNull(exports);
                        imports.AddFromExports(register.@as, exports!);
                        continue;
                    case AssertUninstantiable assert:
                        trapExpected = () =>
                        {
                            try
                            {
                                // Instantiate, not just compile: active data/element-segment bounds checks and the
                                // start function run in the instance constructor, which is where these modules trap.
                                Compile.FromBinary<TExports>(Path.Combine(pathBase, assert.filename))(imports);
                            }
                            catch (TargetInvocationException x) when (x.InnerException != null)
                            {
                                ExceptionDispatchInfo.Capture(x.InnerException).Throw();
                            }
                        };
                        switch (assert.text)
                        {
                            case "unreachable":
                                Assert.ThrowsException<UnreachableException>(trapExpected, $"{command.line}");
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

        if (onFailure == null && skip != null)
            Assert.Inconclusive("Some scenarios were skipped.");
    }

    // Compares a single actual return value against its expected raw value, mirroring the structural-equality
    // fast-path, the approximate float compares, and the lane-wise v128 compare. Returns false only when the
    // expected value type could not be parsed (an inconclusive result the caller propagates).
    static bool CompareReturnValue(TypedValue rawExpected, object? actual, uint line)
    {
        if (Equals(rawExpected.BoxedValue, actual))
            return true;

        switch (rawExpected.type)
        {
            default:
                // This happens in conversion.json starting at "line": 317 when run via GitHub Action but never locally (for me).
                Assert.Inconclusive($"{line}: Failed to parse expected value type.");
                return false;

            case RawValueType.i32:
            case RawValueType.i64:
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
        assert_uninstantiable
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

        public override string ToString() => $"{base.ToString()}: {filename}";
    }

    [JsonConverter(typeof(JsonStringEnumConverter<RawValueType>))]
    enum RawValueType
    {
        i32 = WebAssemblyValueType.Int32,
        i64 = WebAssemblyValueType.Int64,
        f32 = WebAssemblyValueType.Float32,
        f64 = WebAssemblyValueType.Float64,
        // Reference types (WASM 2.0). Library doesn't implement them yet; these exist only so
        // STJ can deserialize the JSON. Tests that reach the runtime path on a ref-type value
        // will fail (caught per-line by Discover or by skip predicates).
        externref = -100,
        funcref = -101,
        v128 = WebAssemblyValueType.V128,
    }

    // Custom converter that accepts both numeric-string values ("0", "4286578688", ...) and the
    // post-2.0 inline NaN markers ("nan:canonical", "nan:arithmetic"), which replaced the older
    // separate assert_return_canonical_nan / assert_return_arithmetic_nan command types.
    // Map each NaN marker to a NaN bit pattern; the runner's existing equality fast-path then
    // matches any NaN result (float.NaN.Equals(float.NaN) is true, regardless of payload).
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
                _ => uint.Parse(s),
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
                _ => ulong.Parse(s),
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
                return ulong.Parse(s);
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

    class AssertUnlinkable() : InvalidCommand(CommandType.assert_unlinkable);

    class AssertUninstantiable() : InvalidCommand(CommandType.assert_uninstantiable);

    class Register() : Command(CommandType.register)
    {
        public string name;
        public string @as;
    }
#pragma warning restore    
}
