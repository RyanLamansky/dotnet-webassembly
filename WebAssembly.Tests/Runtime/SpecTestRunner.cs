using JsonSubTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace WebAssembly.Runtime
{
    static class SpecTestRunner
    {
        public static void Run(string pathBase, string json) => Run<object>(pathBase, json);

        public static void Run(string pathBase, string json, Func<uint, bool> skip) => Run<object>(pathBase, json, skip);

        public static void Run<TExports>(string pathBase, string json)
            where TExports : class
        {
            Run<TExports>(pathBase, json, null);
        }

        public static void Run<TExports>(string pathBase, string json, Func<uint, bool> skip)
            where TExports : class
        {
            TestInfo testInfo;
            using (var reader = new StreamReader(Path.Combine(pathBase, json)))
            {
                var settings = new JsonSerializerSettings();
                settings.Converters.Add(JsonSubtypesConverterBuilder
                    .Of(typeof(Command), "type")
                    .RegisterSubtype(typeof(ModuleCommand), CommandType.module)
                    .RegisterSubtype(typeof(AssertReturn), CommandType.assert_return)
                    .RegisterSubtype(typeof(AssertReturnCanonicalNan), CommandType.assert_return_canonical_nan)
                    .RegisterSubtype(typeof(AssertReturnArithmeticNan), CommandType.assert_return_arithmetic_nan)
                    .RegisterSubtype(typeof(AssertInvalid), CommandType.assert_invalid)
                    .RegisterSubtype(typeof(AssertTrap), CommandType.assert_trap)
                    .RegisterSubtype(typeof(AssertMalformed), CommandType.assert_malformed)
                    .SerializeDiscriminatorProperty()
                    .Build());
                settings.Converters.Add(JsonSubtypesConverterBuilder
                    .Of(typeof(TestAction), "type")
                    .RegisterSubtype(typeof(Invoke), TestActionType.invoke)
                    .SerializeDiscriminatorProperty()
                    .Build());
                settings.Converters.Add(JsonSubtypesConverterBuilder
                    .Of(typeof(TypedValue), "type")
                    .RegisterSubtype(typeof(Int32Value), RawValueType.i32)
                    .RegisterSubtype(typeof(Int64Value), RawValueType.i64)
                    .RegisterSubtype(typeof(Float32Value), RawValueType.f32)
                    .RegisterSubtype(typeof(Float64Value), RawValueType.f64)
                    .SerializeDiscriminatorProperty()
                    .Build());
                testInfo = (TestInfo)JsonSerializer.Create(settings).Deserialize(reader, typeof(TestInfo));
            }

            Instance<TExports> instance = null;
            TExports exports = null;
            Dictionary<string, MethodInfo> methodsByName = null;

            foreach (var command in testInfo.commands)
            {
                if (skip != null && skip(command.line))
                    continue;

                switch (command)
                {
                    case ModuleCommand module:
                        var path = Path.Combine(pathBase, module.filename);
                        Assert.IsNotNull(Module.ReadFromBinary(path)); // Ensure the module parser can read it.
                        instance = Compile.FromBinary<TExports>(path)(new ImportDictionary());
                        exports = instance.Exports;
                        methodsByName = exports.GetType().GetMethods().ToDictionary(m => m.Name);
                        continue;
                    case AssertReturn assert:
                        switch (assert.action)
                        {
                            case Invoke invoke:
                                Assert.IsNotNull(methodsByName);
                                Assert.IsTrue(methodsByName.TryGetValue(invoke.field, out var methodInfo));
                                object result;
                                try
                                {
                                    result = methodInfo.Invoke(exports, invoke.args.Select(arg => arg.BoxedValue).ToArray());
                                }
                                catch (TargetInvocationException x)
                                {
                                    throw new AssertFailedException($"{command.line}: {x.InnerException.Message}", x.InnerException);
                                }
                                catch (Exception x)
                                {
                                    throw new AssertFailedException($"{command.line}: {x.Message}", x);
                                }
                                Assert.AreEqual(assert.expected[0].BoxedValue, result);
                                continue;
                            default:
                                throw new AssertFailedException($"{assert.action} doesn't have a test procedure set up.");
                        }
                    case AssertReturnCanonicalNan assert:
                        switch (assert.action)
                        {
                            case Invoke invoke:
                                Assert.IsNotNull(methodsByName);
                                Assert.IsTrue(methodsByName.TryGetValue(invoke.field, out var methodInfo));
                                var result = methodInfo.Invoke(exports, invoke.args.Select(arg => arg.BoxedValue).ToArray());
                                switch (assert.expected[0].type)
                                {
                                    case RawValueType.f32:
                                        Assert.IsTrue(float.IsNaN((float)result));
                                        continue;
                                    case RawValueType.f64:
                                        Assert.IsTrue(double.IsNaN((double)result));
                                        continue;
                                    default:
                                        throw new AssertFailedException($"{assert.expected[0].type} doesn't support NaN checks.");
                                }
                            default:
                                throw new AssertFailedException($"{assert.action} doesn't have a test procedure set up.");
                        }
                    case AssertReturnArithmeticNan assert:
                        switch (assert.action)
                        {
                            case Invoke invoke:
                                Assert.IsNotNull(methodsByName);
                                Assert.IsTrue(methodsByName.TryGetValue(invoke.field, out var methodInfo));
                                var result = methodInfo.Invoke(exports, invoke.args.Select(arg => arg.BoxedValue).ToArray());
                                switch (assert.expected[0].type)
                                {
                                    case RawValueType.f32:
                                        Assert.IsTrue(float.IsNaN((float)result));
                                        continue;
                                    case RawValueType.f64:
                                        Assert.IsTrue(double.IsNaN((double)result));
                                        continue;
                                    default:
                                        throw new AssertFailedException($"{assert.expected[0].type} doesn't support NaN checks.");
                                }
                            default:
                                throw new AssertFailedException($"{assert.action} doesn't have a test procedure set up.");
                        }
                    case AssertInvalid assert:
                        {
                            Action trapExpected = () =>
                            {
                                try
                                {
                                    Compile.FromBinary<TExports>(Path.Combine(pathBase, assert.filename));
                                }
                                catch (TargetInvocationException x)
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
                                    catch (Exception x)
                                    {
                                        throw new AssertFailedException($"{command.line} threw an unexpected exception of type {x.GetType().Name}.");
                                    }
                                    throw new AssertFailedException($"{command.line} should have thrown an exception but did not.");
                                default:
                                    throw new AssertFailedException($"{assert.text} doesn't have a test procedure set up.");
                            }
                        }
                    case AssertTrap assert:
                        switch (assert.action)
                        {
                            case Invoke invoke:
                                Action trapExpected = () =>
                                {
                                    Assert.IsNotNull(methodsByName);
                                    Assert.IsTrue(methodsByName.TryGetValue(invoke.field, out var methodInfo));
                                    try
                                    {
                                        methodInfo.Invoke(exports, invoke.args.Select(arg => arg.BoxedValue).ToArray());
                                    }
                                    catch (TargetInvocationException x)
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
                                    default:
                                        throw new AssertFailedException($"{command.line}: {assert.text} doesn't have a test procedure set up.");
                                }
                            default:
                                throw new AssertFailedException($"{command.line}: {assert.action} doesn't have a test procedure set up.");
                        }
                    case AssertMalformed assert:
                        continue; // Not writing a WAT parser.
                    default:
                        throw new AssertFailedException($"{command.line}: {command} doesn't have a test procedure set up.");
                }
            }

            if (skip != null)
                Assert.Inconclusive("Some scenarios were skipped.");
        }

        [JsonConverter(typeof(StringEnumConverter))]
        enum CommandType
        {
            module,
            assert_return,
            assert_return_canonical_nan,
            assert_return_arithmetic_nan,
            assert_invalid,
            assert_trap,
            assert_malformed,
        }

#pragma warning disable 649
        class TestInfo
        {
            public string source_filename;
            public Command[] commands;
        }

        abstract class Command
        {
            public CommandType type;
            public uint line;

            public override string ToString() => type.ToString();
        }

        class ModuleCommand : Command
        {
            public string filename;

            public override string ToString() => $"{base.ToString()}: {filename}";
        }

        [JsonConverter(typeof(StringEnumConverter))]
        enum RawValueType
        {
            i32 = WebAssemblyValueType.Int32,
            i64 = WebAssemblyValueType.Int64,
            f32 = WebAssemblyValueType.Float32,
            f64 = WebAssemblyValueType.Float64,
        }

        class TypeOnly
        {
            public RawValueType type;

            public override string ToString() => type.ToString();
        }

        abstract class TypedValue : TypeOnly
        {
            public abstract object BoxedValue { get; }
        }

        class Int32Value : TypedValue
        {
            public uint value;

            public override object BoxedValue => (int)value;

            public override string ToString() => $"{type}: {value}";
        }

        class Int64Value : TypedValue
        {
            public ulong value;

            public override object BoxedValue => (long)value;

            public override string ToString() => $"{type}: {value}";
        }

        class Float32Value : Int32Value
        {
            public override object BoxedValue => BitConverter.Int32BitsToSingle(unchecked((int)value));

            public override string ToString() => $"{type}: {BoxedValue}";
        }

        class Float64Value : Int64Value
        {
            public override object BoxedValue => BitConverter.Int64BitsToDouble(unchecked((long)value));

            public override string ToString() => $"{type}: {BoxedValue}";
        }

        [JsonConverter(typeof(StringEnumConverter))]
        enum TestActionType
        {
            invoke,
        }

        abstract class TestAction
        {
            public TestActionType type;
        }

        class Invoke : TestAction
        {
            public string field;
            public TypedValue[] args;

            public override string ToString() => $"{field} [{string.Join(',', (IEnumerable<TypedValue>)args)}]";
        }

        abstract class AssertCommand : Command
        {
            public TestAction action;

            public override string ToString() => $"{base.ToString()}: {action.ToString()}";
        }

        class AssertReturn : AssertCommand
        {
            public TypedValue[] expected;

            public override string ToString() => $"{base.ToString()} = [{string.Join(',', (IEnumerable<TypedValue>)expected)}]";
        }

        class AssertReturnCanonicalNan : AssertCommand
        {
            public TypeOnly[] expected;

            public override string ToString() => $"{base.ToString()} = [{string.Join(',', (IEnumerable<TypeOnly>)expected)}]";
        }

        class AssertReturnArithmeticNan : AssertCommand
        {
            public TypeOnly[] expected;

            public override string ToString() => $"{base.ToString()} = [{string.Join(',', (IEnumerable<TypeOnly>)expected)}]";
        }

        class AssertInvalid : Command
        {
            public string filename;
            public string text;
            public string module_type;

            public override string ToString() => $"{base.ToString()}: {filename} \"{text}\" {module_type}";
        }

        class AssertTrap : AssertCommand
        {
            public TypeOnly[] expected;
            public string text;
        }

        class AssertMalformed : Command
        {
            public string filename;
            public string text;
            public string module_type;
        }
#pragma warning restore
    }
}
