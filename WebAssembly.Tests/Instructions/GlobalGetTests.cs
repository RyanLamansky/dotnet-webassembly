using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Tests the <see cref="GlobalGet"/> instruction.
    /// </summary>
    [TestClass]
    public class GlobalGetTests
    {
        /// <summary>
        /// Used to test a single return with no parameters.
        /// </summary>
        public abstract class TestBase
        {
            /// <summary>
            /// Returns a value.
            /// </summary>
            public abstract int TestInt32();

            /// <summary>
            /// Returns a value.
            /// </summary>
            public abstract long TestInt64();

            /// <summary>
            /// Returns a value.
            /// </summary>
            public abstract float TestFloat32();

            /// <summary>
            /// Returns a value.
            /// </summary>
            public abstract double TestFloat64();
        }

        /// <summary>
        /// Tests compilation and execution of the <see cref="GlobalGet"/> instruction for immutable values.
        /// </summary>
        [TestMethod]
        public void GetGlobal_Immutable_Compiled()
        {
            var module = new Module();
            module.Types.Add(new WebAssemblyType
            {
                Parameters = Array.Empty<WebAssemblyValueType>(),
                Returns = new[]
                {
                    WebAssemblyValueType.Int32,
                }
            });
            module.Types.Add(new WebAssemblyType
            {
                Parameters = Array.Empty<WebAssemblyValueType>(),
                Returns = new[]
                {
                    WebAssemblyValueType.Int64,
                }
            });
            module.Types.Add(new WebAssemblyType
            {
                Parameters = Array.Empty<WebAssemblyValueType>(),
                Returns = new[]
                {
                    WebAssemblyValueType.Float32,
                }
            });
            module.Types.Add(new WebAssemblyType
            {
                Parameters = Array.Empty<WebAssemblyValueType>(),
                Returns = new[]
                {
                    WebAssemblyValueType.Float64,
                }
            });
            module.Functions.Add(new Function(0));
            module.Functions.Add(new Function(1));
            module.Functions.Add(new Function(2));
            module.Functions.Add(new Function(3));
            module.Globals.Add(new Global(WebAssemblyValueType.Int32, new Int32Constant(4), new End()));
            module.Globals.Add(new Global(WebAssemblyValueType.Int64, new Int64Constant(5), new End()));
            module.Globals.Add(new Global(WebAssemblyValueType.Float32, new Float32Constant(6), new End()));
            module.Globals.Add(new Global(WebAssemblyValueType.Float64, new Float64Constant(7), new End()));
            module.Exports.Add(new Export(nameof(TestBase.TestInt32), 0));
            module.Exports.Add(new Export(nameof(TestBase.TestInt64), 1));
            module.Exports.Add(new Export(nameof(TestBase.TestFloat32), 2));
            module.Exports.Add(new Export(nameof(TestBase.TestFloat64), 3));
            module.Codes.Add(new FunctionBody(new GlobalGet(0), new End()));
            module.Codes.Add(new FunctionBody(new GlobalGet(1), new End()));
            module.Codes.Add(new FunctionBody(new GlobalGet(2), new End()));
            module.Codes.Add(new FunctionBody(new GlobalGet(3), new End()));

            var compiled = module.ToInstance<TestBase>();

            var exports = compiled.Exports;
            Assert.AreEqual(4, exports.TestInt32());
            Assert.AreEqual(5, exports.TestInt64());
            Assert.AreEqual(6, exports.TestFloat32());
            Assert.AreEqual(7, exports.TestFloat64());
        }

        /// <summary>
        /// Tests compilation and execution of the <see cref="GlobalGet"/> instruction for mutable values.
        /// </summary>
        [TestMethod]
        public void GetGlobal_Mutable_Compiled()
        {
            var module = new Module();
            module.Types.Add(new WebAssemblyType
            {
                Parameters = Array.Empty<WebAssemblyValueType>(),
                Returns = new[]
                {
                    WebAssemblyValueType.Int32,
                }
            });
            module.Types.Add(new WebAssemblyType
            {
                Parameters = Array.Empty<WebAssemblyValueType>(),
                Returns = new[]
                {
                    WebAssemblyValueType.Int64,
                }
            });
            module.Types.Add(new WebAssemblyType
            {
                Parameters = Array.Empty<WebAssemblyValueType>(),
                Returns = new[]
                {
                    WebAssemblyValueType.Float32,
                }
            });
            module.Types.Add(new WebAssemblyType
            {
                Parameters = Array.Empty<WebAssemblyValueType>(),
                Returns = new[]
                {
                    WebAssemblyValueType.Float64,
                }
            });
            module.Functions.Add(new Function
            {
                Type = 0,
            });
            module.Functions.Add(new Function
            {
                Type = 1,
            });
            module.Functions.Add(new Function
            {
                Type = 2,
            });
            module.Functions.Add(new Function
            {
                Type = 3,
            });
            module.Globals.Add(new Global
            {
                IsMutable = true,
                ContentType = WebAssemblyValueType.Int32,
                InitializerExpression = new Instruction[]
                {
                    new Int32Constant(4),
                    new End(),
                },
            });
            module.Globals.Add(new Global
            {
                IsMutable = true,
                ContentType = WebAssemblyValueType.Int64,
                InitializerExpression = new Instruction[]
                {
                    new Int64Constant(5),
                    new End(),
                },
            });
            module.Globals.Add(new Global
            {
                IsMutable = true,
                ContentType = WebAssemblyValueType.Float32,
                InitializerExpression = new Instruction[]
                {
                    new Float32Constant(6),
                    new End(),
                },
            });
            module.Globals.Add(new Global
            {
                IsMutable = true,
                ContentType = WebAssemblyValueType.Float64,
                InitializerExpression = new Instruction[]
                {
                    new Float64Constant(7),
                    new End(),
                },
            });
            module.Exports.Add(new Export
            {
                Index = 0,
                Name = nameof(TestBase.TestInt32)
            });
            module.Exports.Add(new Export
            {
                Index = 1,
                Name = nameof(TestBase.TestInt64)
            });
            module.Exports.Add(new Export
            {
                Index = 2,
                Name = nameof(TestBase.TestFloat32)
            });
            module.Exports.Add(new Export
            {
                Index = 3,
                Name = nameof(TestBase.TestFloat64)
            });
            module.Codes.Add(new FunctionBody
            {
                Code = new Instruction[]
                {
                    new GlobalGet(0),
                    new End(),
                },
            });
            module.Codes.Add(new FunctionBody
            {
                Code = new Instruction[]
                {
                    new GlobalGet(1),
                    new End(),
                },
            });
            module.Codes.Add(new FunctionBody
            {
                Code = new Instruction[]
                {
                    new GlobalGet(2),
                    new End(),
                },
            });
            module.Codes.Add(new FunctionBody
            {
                Code = new Instruction[]
                {
                    new GlobalGet(3),
                    new End(),
                },
            });

            var compiled = module.ToInstance<TestBase>();

            var exports = compiled.Exports;
            Assert.AreEqual(4, exports.TestInt32());
            Assert.AreEqual(5, exports.TestInt64());
            Assert.AreEqual(6, exports.TestFloat32());
            Assert.AreEqual(7, exports.TestFloat64());
        }

        /// <summary>
        /// Always returns 3, used by <see cref="GetGlobal_Imported_Compiled"/>
        /// </summary>
        public static int ImportedImmutableGlobalReturns3 => 3;

        /// <summary>
        /// Tests that imported globals can be read.
        /// </summary>
        [TestMethod]
        public void GetGlobal_Imported_Compiled()
        {
            var module = new Module();
            module.Types.Add(new WebAssemblyType
            {
                Returns = new[]
                {
                    WebAssemblyValueType.Int32,
                }
            });
            module.Functions.Add(new Function
            {
            });
            module.Imports.Add(new Import.Global
            {
                Module = "Imported",
                Field = "Global",
                ContentType = WebAssemblyValueType.Int32,
            });
            module.Exports.Add(new Export
            {
                Name = nameof(CompilerTestBase<int>.Test)
            });
            module.Codes.Add(new FunctionBody
            {
                Code = new Instruction[]
                {
                    new GlobalGet(0),
                    new End(),
                },
            });

            var compiled = module.ToInstance<CompilerTestBase0<int>>(
                new ImportDictionary {
                    { "Imported", "Global", new GlobalImport(() => ImportedImmutableGlobalReturns3) },
                });

            Assert.AreEqual(ImportedImmutableGlobalReturns3, compiled.Exports.Test());
        }

        /// <summary>
        /// Used by <see cref="GetGlobal_SecondExportFirstGlobal_Compiled"/>.
        /// </summary>
        public abstract class GlobalAndFunctionExport
        {
            /// <summary>
            /// A test function.
            /// </summary>
            public abstract int TestFunction();

            /// <summary>
            /// A test global.
            /// </summary>
            public abstract int TestGlobal { get; }
        }

        /// <summary>
        /// Verifies that global exports work if there is another export ahead of it.
        /// </summary>
        [TestMethod]
        public void GetGlobal_SecondExportFirstGlobal_Compiled()
        {
            var module = new Module();
            module.Types.Add(new WebAssemblyType
            {
                Parameters = Array.Empty<WebAssemblyValueType>(),
                Returns = new[] { WebAssemblyValueType.Int32 },
            });
            module.Globals.Add(new Global
            {
                ContentType = WebAssemblyValueType.Int32,
                IsMutable = true,
                InitializerExpression = new Instruction[]
                {
                    new Int32Constant(5),
                    new End()
                }
            });
            module.Exports.Add(new Export
            {
                Index = 0,
                Kind = ExternalKind.Function,
                Name = nameof(GlobalAndFunctionExport.TestFunction),
            });
            module.Exports.Add(new Export
            {
                Index = 0,
                Kind = ExternalKind.Global,
                Name = nameof(GlobalAndFunctionExport.TestGlobal),
            });
            module.Functions.Add(new Function());
            module.Codes.Add(new FunctionBody
            {
                Code = new Instruction[]
                {
                    new GlobalGet(0),
                    new End()
                },
            });

            var exports = module.ToInstance<GlobalAndFunctionExport>().Exports;
            Assert.AreEqual(5, exports.TestGlobal);
            Assert.AreEqual(5, exports.TestFunction());
        }
    }
}