using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Tests the <see cref="GetGlobal"/> instruction.
    /// </summary>
    [TestClass]
    public class GetGlobalTests
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
        /// Tests compilation and execution of the <see cref="GetGlobal"/> instruction for immutable values.
        /// </summary>
        [TestMethod]
        public void GetGlobal_Immutable_Compiled()
        {
            var module = new Module();
            module.Types.Add(new Type
            {
                Parameters = new ValueType[]
                {
                },
                Returns = new[]
                {
                    ValueType.Int32,
                }
            });
            module.Types.Add(new Type
            {
                Parameters = new ValueType[]
                {
                },
                Returns = new[]
                {
                    ValueType.Int64,
                }
            });
            module.Types.Add(new Type
            {
                Parameters = new ValueType[]
                {
                },
                Returns = new[]
                {
                    ValueType.Float32,
                }
            });
            module.Types.Add(new Type
            {
                Parameters = new ValueType[]
                {
                },
                Returns = new[]
                {
                    ValueType.Float64,
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
                ContentType = ValueType.Int32,
                InitializerExpression = new Instruction[]
                {
                    new Int32Constant(4),
                    new End(),
                },
            });
            module.Globals.Add(new Global
            {
                ContentType = ValueType.Int64,
                InitializerExpression = new Instruction[]
                {
                    new Int64Constant(5),
                    new End(),
                },
            });
            module.Globals.Add(new Global
            {
                ContentType = ValueType.Float32,
                InitializerExpression = new Instruction[]
                {
                    new Float32Constant(6),
                    new End(),
                },
            });
            module.Globals.Add(new Global
            {
                ContentType = ValueType.Float64,
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
                    new GetGlobal(0),
                    new End(),
                },
            });
            module.Codes.Add(new FunctionBody
            {
                Code = new Instruction[]
                {
                    new GetGlobal(1),
                    new End(),
                },
            });
            module.Codes.Add(new FunctionBody
            {
                Code = new Instruction[]
                {
                    new GetGlobal(2),
                    new End(),
                },
            });
            module.Codes.Add(new FunctionBody
            {
                Code = new Instruction[]
                {
                    new GetGlobal(3),
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
        /// Tests compilation and execution of the <see cref="GetGlobal"/> instruction for mutable values.
        /// </summary>
        [TestMethod]
        public void GetGlobal_Mutable_Compiled()
        {
            var module = new Module();
            module.Types.Add(new Type
            {
                Parameters = new ValueType[]
                {
                },
                Returns = new[]
                {
                    ValueType.Int32,
                }
            });
            module.Types.Add(new Type
            {
                Parameters = new ValueType[]
                {
                },
                Returns = new[]
                {
                    ValueType.Int64,
                }
            });
            module.Types.Add(new Type
            {
                Parameters = new ValueType[]
                {
                },
                Returns = new[]
                {
                    ValueType.Float32,
                }
            });
            module.Types.Add(new Type
            {
                Parameters = new ValueType[]
                {
                },
                Returns = new[]
                {
                    ValueType.Float64,
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
                ContentType = ValueType.Int32,
                InitializerExpression = new Instruction[]
                {
                    new Int32Constant(4),
                    new End(),
                },
            });
            module.Globals.Add(new Global
            {
                IsMutable = true,
                ContentType = ValueType.Int64,
                InitializerExpression = new Instruction[]
                {
                    new Int64Constant(5),
                    new End(),
                },
            });
            module.Globals.Add(new Global
            {
                IsMutable = true,
                ContentType = ValueType.Float32,
                InitializerExpression = new Instruction[]
                {
                    new Float32Constant(6),
                    new End(),
                },
            });
            module.Globals.Add(new Global
            {
                IsMutable = true,
                ContentType = ValueType.Float64,
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
                    new GetGlobal(0),
                    new End(),
                },
            });
            module.Codes.Add(new FunctionBody
            {
                Code = new Instruction[]
                {
                    new GetGlobal(1),
                    new End(),
                },
            });
            module.Codes.Add(new FunctionBody
            {
                Code = new Instruction[]
                {
                    new GetGlobal(2),
                    new End(),
                },
            });
            module.Codes.Add(new FunctionBody
            {
                Code = new Instruction[]
                {
                    new GetGlobal(3),
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
            module.Types.Add(new Type
            {
                Returns = new[]
                {
                    ValueType.Int32,
                }
            });
            module.Functions.Add(new Function
            {
            });
            module.Imports.Add(new Import.Global
            {
                Module = "Imported",
                Field = "Global",
                ContentType = ValueType.Int32,
            });
            module.Exports.Add(new Export
            {
                Name = nameof(CompilerTestBase<int>.Test)
            });
            module.Codes.Add(new FunctionBody
            {
                Code = new Instruction[]
                {
                    new GetGlobal(0),
                    new End(),
                },
            });

            var compiled = module.ToInstance<CompilerTestBase0<int>>(
                new ImportDictionary {
                    { "Imported", "Global", new GlobalImport(() => ImportedImmutableGlobalReturns3) },
                });

            Assert.AreEqual(ImportedImmutableGlobalReturns3, compiled.Exports.Test());
        }
    }
}