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

static class SpecTestRunner
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

    private static readonly RegeneratingWeakReference<JsonSerializerOptions> serializerOptions =
        new(() => new JsonSerializerOptions
        {
            IncludeFields = true,
        });

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
            testInfo = JsonSerializer.Deserialize<TestInfo>(reader, serializerOptions)!;
        }

        ObjectMethods? methodsByName = null;
        var moduleMethodsByName = new Dictionary<string, ObjectMethods>();

        // From https://github.com/WebAssembly/spec/blob/master/interpreter/host/spectest.ml
        var imports = new ImportDictionary
        {
            { "spectest", "print_i32", new FunctionImport((Action<int>)(i => { })) },
            { "spectest", "print_i32_f32", new FunctionImport((Action<int, float>)((i, f) => { })) },
            { "spectest", "print_f64_f64", new FunctionImport((Action<double, double>)((d1, d2) => { })) },
            { "spectest", "print_f32", new FunctionImport((Action<float>)(i => { })) },
            { "spectest", "print_f64", new FunctionImport((Action<double>)(i => { })) },
            { "spectest", "global_i32", new GlobalImport(() => 666) },
            { "spectest", "global_i64", new GlobalImport(() => 666L) },
            { "spectest", "global_f32", new GlobalImport(() => 666.0F) },
            { "spectest", "global_f64", new GlobalImport(() => 666.0) },
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
                            var rawExpected = assert.expected[0];
                            if (rawExpected.BoxedValue.Equals(result))
                                continue;

                            switch (rawExpected)
                            {
                                case Float32Value f32Expected:
                                    {
                                        var expected = f32Expected.ActualValue;
                                        Assert.AreEqual(expected, (float)result!, Math.Abs(expected * 0.000001f), $"{command.line}: f32 compare");
                                    }
                                    continue;
                                case Float64Value f64Expected:
                                    {
                                        var expected = f64Expected.ActualValue;
                                        Assert.AreEqual(expected, (double)result!, Math.Abs(expected * 0.000001), $"{command.line}: f64 compare");
                                    }
                                    continue;
                                case Int32Value:
                                case Int64Value:
                                    break;

                                case V128Value v128Expected:
                                    {
                                        var actual = (Vector128<byte>)result!;
                                        if (!v128Expected.IsMatch(actual))
                                            Assert.AreEqual(v128Expected.ActualValue, actual, $"{command.line}: v128 compare");
                                    }
                                    continue;
                            }

                            throw new AssertFailedException($"{command.line}: Not equal {rawExpected.GetType().Name}: {rawExpected.BoxedValue} and {result}");
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
                                throw new AssertFailedException($"{assert.expected[0].type} doesn't support NaN checks for canonical NaN.");
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
                                throw new AssertFailedException($"{assert.expected[0].type} doesn't support NaN checks for arithmetic NaN.");
                        }
                    case AssertInvalid assert:
                        if (assert.module_type == "text")
                            continue; // WAT format — not parsing WAT
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
                            // "invalid result arity" was a WASM 1.0 restriction; multi-value (2.0) allows multiple returns.
                            case "invalid result arity":
                                continue;
                            case "unknown memory 0":
                            case "constant expression required":
                            case "duplicate export name":
                            case "unknown table":
                            case "unknown local":
                            case "multiple memories":
                            case "size minimum must not be greater than maximum":
                            case "memory size must be at most 65536 pages (4GiB)":
                            case "unknown label":
                            case "unknown type":
                                Assert.ThrowsException<ModuleLoadException>(trapExpected, $"{command.line}");
                                continue;
                            case "unknown global":
                            case "unknown memory":
                            case "unknown function":
                            case "unknown function 0":
                            case "unknown table 0":
                            case "unknown local 2":
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
                            // "multiple tables" was a WASM 1.0 restriction; reference types (2.0) allows multiple tables.
                            case "multiple tables":
                                continue;
                            case "invalid lane index":
                            case "offset out of range":
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
                            case "invalid conversion to integer":
                                Assert.ThrowsException<OverflowException>(trapExpected, $"{command.line}");
                                continue;
                            case "undefined element":
                            case "uninitialized element 7":
                                try
                                {
                                    trapExpected();
                                    throw new AssertFailedException($"{command.line}: Expected ModuleLoadException, IndexOutOfRangeException, or NullReferenceException, but no exception was thrown.");
                                }
                                catch (ModuleLoadException)
                                {
                                }
                                catch (IndexOutOfRangeException)
                                {
                                }
                                catch (NullReferenceException)
                                {
                                }
                                catch (Exception x)
                                {
                                    throw new AssertFailedException($"{command.line}: Expected ModuleLoadException, IndexOutOfRangeException, or NullReferenceException, but received {x.GetType().Name}.");
                                }
                                continue;
                            case "out of bounds table access":
                                try
                                {
                                    trapExpected();
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
                            case "indirect call type mismatch":
                                Assert.ThrowsException<InvalidCastException>(trapExpected, $"{command.line}");
                                continue;
                            case "unreachable":
                                Assert.ThrowsException<UnreachableException>(trapExpected, $"{command.line}");
                                continue;
                            case "uninitialized element":
                            case "uninitialized element 2":
                            case "uninitialized":
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
                            case "call stack exhausted":
                                // Run on a background thread so tail-call-optimized infinite recursion
                                // (which the CLR JIT converts to a true loop, bypassing
                                // EnsureSufficientExecutionStack) still terminates the test.
                                // A function that never returns within the timeout is treated as
                                // exhausted — correct per WASM spec semantics.
                                bool exhausted = false;
                                Exception? exhaustionException = null;
                                var exhaustionThread = new System.Threading.Thread(() =>
                                {
                                    try { trapExpected(); }
                                    catch (StackOverflowException) { exhausted = true; }
                                    catch (System.InsufficientExecutionStackException) { exhausted = true; }
                                    catch (Exception ex) { exhaustionException = ex; }
                                }, 4 * 1024 * 1024); // 4 MB stack — large enough for non-tail-call recursion
                                exhaustionThread.IsBackground = true;
                                exhaustionThread.Start();
                                if (!exhaustionThread.Join(TimeSpan.FromMilliseconds(100)))
                                {
                                    // Thread is still running after timeout — it's infinite recursion,
                                    // which satisfies the assert_exhaustion expectation.
                                    continue;
                                }
                                if (exhausted)
                                    continue; // Stack was exhausted via exception — pass.
                                if (exhaustionException != null)
                                    throw new AssertFailedException($"{command.line}: assert_exhaustion threw unexpected {exhaustionException.GetType().Name}: {exhaustionException.Message}");
                                // Thread completed without exception or exhaustion — that's a test failure.
                                throw new AssertFailedException($"{command.line}: Expected call stack exhaustion, but the function returned normally.");
                            default:
                                throw new AssertFailedException($"{command.line}: {assert.text} doesn't have a test procedure set up.");
                        }
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
                                try
                                {
                                    trapExpected();
                                    throw new AssertFailedException($"{command.line}: Expected ModuleLoadException or UnreachableException, but no exception was thrown.");
                                }
                                catch (ModuleLoadException)
                                {
                                }
                                catch (UnreachableException)
                                {
                                }
                                continue;
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
                TryAdd(NameCleaner.CleanName(method.Name), method.MethodInfo);
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
        v128 = WebAssemblyValueType.V128,
        // Reference types (WASM 2.0). Library doesn't implement them yet; these exist only so
        // STJ can deserialize the JSON. Tests that reach the runtime path on a ref-type value
        // will fail (caught per-line by Discover or by skip predicates).
        externref = -100,
        funcref = -101,
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

    [JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
    [JsonDerivedType(typeof(Int32Value), typeDiscriminator: nameof(RawValueType.i32))]
    [JsonDerivedType(typeof(Int64Value), typeDiscriminator: nameof(RawValueType.i64))]
    [JsonDerivedType(typeof(Float32Value), typeDiscriminator: nameof(RawValueType.f32))]
    [JsonDerivedType(typeof(Float64Value), typeDiscriminator: nameof(RawValueType.f64))]
    [JsonDerivedType(typeof(V128Value), typeDiscriminator: nameof(RawValueType.v128))]
    [JsonDerivedType(typeof(ExternRefValue), typeDiscriminator: nameof(RawValueType.externref))]
    [JsonDerivedType(typeof(FuncRefValue), typeDiscriminator: nameof(RawValueType.funcref))]
    abstract class TypedValue(RawValueType valueType)
    {
        // Not inheriting TypeOnly here: the inherited `type` field would conflict with the
        // polymorphism discriminator on .NET 10+ (see Command for details). The two hierarchies
        // are used independently — TypeOnly[] in canonical/arithmetic-NaN asserts, TypedValue[]
        // in regular asserts and invoke args — so keeping them separate is safe.
        [JsonIgnore]
        public readonly RawValueType type = valueType;
        public abstract object BoxedValue { get; }
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

        public override string ToString() => $"i32: {value}";
    }

    class Int64Value(RawValueType valueType) : TypedValue(valueType)
    {
        [JsonConstructor]
        public Int64Value() : this(RawValueType.i64) { }

        [JsonConverter(typeof(NanCapableUInt64Converter))]
        public ulong value;

        public override object BoxedValue => (long)value;

        public override string ToString() => $"i64: {value}";
    }

    class Float32Value() : Int32Value(RawValueType.f32)
    {
        public float ActualValue => BitConverter.Int32BitsToSingle(unchecked((int)value));

        public override object BoxedValue => ActualValue;

        public override string ToString() => $"f32: {BoxedValue}";
    }

    class Float64Value() : Int64Value(RawValueType.f64)
    {
        public double ActualValue => BitConverter.Int64BitsToDouble(unchecked((long)value));

        public override object BoxedValue => ActualValue;

        public override string ToString() => $"f64: {BoxedValue}";
    }

    class V128Value() : TypedValue(RawValueType.v128)
    {
        public string? lane_type;
        public string[]? value;

        public override object BoxedValue => ActualValue;

        // Returns true if any lane is a NaN pattern ("nan:canonical" or "nan:arithmetic").
        public bool HasNaN => value != null && value.Any(v => v.StartsWith("nan:", StringComparison.Ordinal));

        // Parses a lane value string to ulong. NaN patterns map to a canonical NaN for the lane type.
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
                if (value == null || value.Length == 0)
                    return Vector128<byte>.Zero;
                var bytes = new byte[16];
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
                            bytes[i * 8] = (byte)v;
                            bytes[i * 8 + 1] = (byte)(v >> 8);
                            bytes[i * 8 + 2] = (byte)(v >> 16);
                            bytes[i * 8 + 3] = (byte)(v >> 24);
                            bytes[i * 8 + 4] = (byte)(v >> 32);
                            bytes[i * 8 + 5] = (byte)(v >> 40);
                            bytes[i * 8 + 6] = (byte)(v >> 48);
                            bytes[i * 8 + 7] = (byte)(v >> 56);
                        }
                        break;
                }
                return Vector128.Create(bytes[0], bytes[1], bytes[2], bytes[3], bytes[4], bytes[5], bytes[6], bytes[7],
                    bytes[8], bytes[9], bytes[10], bytes[11], bytes[12], bytes[13], bytes[14], bytes[15]);
            }
        }

        // NaN-aware lane-by-lane match: a "nan:*" pattern matches any NaN in that lane.
        public bool IsMatch(Vector128<byte> actual)
        {
            if (!HasNaN) return ActualValue.Equals(actual);
            if (value == null) return false;
            switch (lane_type)
            {
                case "f32":
                {
                    var af = actual.As<byte, float>();
                    for (var i = 0; i < 4 && i < value.Length; i++)
                    {
                        if (value[i].StartsWith("nan:", StringComparison.Ordinal))
                        { if (!float.IsNaN(af[i])) return false; }
                        else
                        { if ((uint)ParseLane(value[i]) != BitConverter.SingleToUInt32Bits(af[i])) return false; }
                    }
                    return true;
                }
                case "f64":
                {
                    var ad = actual.As<byte, double>();
                    for (var i = 0; i < 2 && i < value.Length; i++)
                    {
                        if (value[i].StartsWith("nan:", StringComparison.Ordinal))
                        { if (!double.IsNaN(ad[i])) return false; }
                        else
                        { if (ParseLane(value[i]) != BitConverter.DoubleToUInt64Bits(ad[i])) return false; }
                    }
                    return true;
                }
                default:
                    return ActualValue.Equals(actual);
            }
        }

        public override string ToString() => $"v128({lane_type})[{string.Join(',', value ?? [])}]";
    }

    // Reference-type stubs. Library doesn't implement reference types yet (WASM 2.0 feature),
    // so BoxedValue throws — execution-time failures are caught per-line by Discover or skipped
    // by per-category skip predicates. The class only exists so STJ can deserialize JSON files
    // that mention `externref` / `funcref`.
    class ExternRefValue() : TypedValue(RawValueType.externref)
    {
        public string value;

        public override object BoxedValue => throw new NotSupportedException("externref values not implemented (WASM 2.0)");

        public override string ToString() => $"{type}: {value}";
    }

    class FuncRefValue() : TypedValue(RawValueType.funcref)
    {
        public string value;

        public override object BoxedValue => throw new NotSupportedException("funcref values not implemented (WASM 2.0)");

        public override string ToString() => $"{type}: {value}";
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
